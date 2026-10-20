using AutoMapper;
using Gmach.Core.DTOs.LoanRequests;
using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;
using Gmach.Core.Interfaces;
using Gmach.Core.Settings;
using Gmach.Service.Mapping;
using Gmach.Service.Services;
using Moq;
using Xunit;

namespace Gmach.Tests
{
    // בדיקות ל-Service בלבד: ה-Repositories מוחלפים ב-Moq, ולכן אין בסיס נתונים.
    public class LoanRequestServiceTests
    {
        private readonly Mock<ILoanRequestRepository> _requests = new();
        private readonly Mock<ILoanRepository> _loans = new();
        private readonly ScoringSettings _scoring = new()
        {
            OnTimeReturnPoints = 10,
            LateReturnPenalty = 20
        };
        private readonly LoanRequestService _service;

        public LoanRequestServiceTests()
        {
            var mapper = new MapperConfiguration(
                c => c.AddProfile<LoanRequestMappingProfile>()).CreateMapper();

            _service = new LoanRequestService(
                _requests.Object, _loans.Object, mapper, _scoring);
        }

        private static Loan NewLoan(LoanStatus status = LoanStatus.Available) => new()
        {
            Id = 1,
            Amount = 1000,
            DueDate = DateTime.UtcNow.AddMonths(1),
            Status = status
        };

        // ---------- הגשת בקשה: משאב מוגבל ----------

        [Fact]
        public async Task Submit_AvailableLoan_ReservesLoanAndCreatesPendingRequest()
        {
            var loan = NewLoan();
            _loans.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(loan);
            _requests.Setup(r => r.HasOpenRequestAsync(7, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            var result = await _service.SubmitAsync(
                7, new SubmitLoanRequestDto { LoanId = 1, Note = "  שלום  " });

            Assert.Equal(LoanStatus.Reserved, loan.Status);
            Assert.Equal("Pending", result.Status);
            Assert.Equal("שלום", result.Note);
            _requests.Verify(r => r.AddAsync(
                It.Is<LoanRequest>(x => x.UserId == 7 && x.RequestedAmount == 1000),
                It.IsAny<CancellationToken>()), Times.Once);
            _requests.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Submit_LoanAlreadyReserved_ThrowsConflictAndSavesNothing()
        {
            var loan = NewLoan(LoanStatus.Reserved);
            _loans.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(loan);
            _requests.Setup(r => r.HasOpenRequestAsync(7, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            await Assert.ThrowsAsync<ConflictException>(() =>
                _service.SubmitAsync(7, new SubmitLoanRequestDto { LoanId = 1 }));

            _requests.Verify(r => r.AddAsync(It.IsAny<LoanRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            _requests.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Submit_UserAlreadyHasOpenRequest_ThrowsConflict()
        {
            _loans.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(NewLoan());
            _requests.Setup(r => r.HasOpenRequestAsync(7, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(() =>
                _service.SubmitAsync(7, new SubmitLoanRequestDto { LoanId = 1 }));
        }

        [Fact]
        public async Task Submit_LoanDoesNotExist_ThrowsNotFound()
        {
            _loans.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Loan?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.SubmitAsync(7, new SubmitLoanRequestDto { LoanId = 99 }));
        }

        // ---------- החלטות מנהל ----------

        private LoanRequest PendingRequest()
        {
            var loan = NewLoan(LoanStatus.Reserved);
            var request = new LoanRequest
            {
                Id = 5,
                UserId = 7,
                User = new User { Id = 7, FirstName = "דנה", LastName = "כהן", Score = 5 },
                Loan = loan,
                LoanId = loan.Id,
                RequestedAmount = loan.Amount
            };
            _requests.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(request);
            return request;
        }

        [Fact]
        public async Task Approve_PendingRequest_ActivatesLoanAndKeepsDueDate()
        {
            var request = PendingRequest();
            var dueDate = request.Loan.DueDate;

            var result = await _service.ApproveAsync(5);

            Assert.Equal("Approved", result.Status);
            Assert.Equal(LoanStatus.Active, request.Loan.Status);
            Assert.Equal(dueDate, request.ApprovedDueDate);
        }

        [Fact]
        public async Task Approve_AlreadyApproved_ThrowsConflict()
        {
            var request = PendingRequest();
            request.Status = LoanRequestStatus.Approved;

            await Assert.ThrowsAsync<ConflictException>(() => _service.ApproveAsync(5));
        }

        [Fact]
        public async Task Reject_WithReason_ReleasesLoanAndStoresReason()
        {
            var request = PendingRequest();

            var result = await _service.RejectAsync(
                5, new RejectLoanRequestDto { Reason = "  אין התאמה  " });

            Assert.Equal("Rejected", result.Status);
            Assert.Equal("אין התאמה", result.RejectionReason);
            Assert.Equal(LoanStatus.Available, request.Loan.Status);
        }

        [Fact]
        public async Task Reject_BlankReason_ThrowsBusinessValidation()
        {
            PendingRequest();

            await Assert.ThrowsAsync<BusinessValidationException>(() =>
                _service.RejectAsync(5, new RejectLoanRequestDto { Reason = "   " }));
        }

        // ---------- החזר וניקוד ----------

        private LoanRequest ApprovedRequest(DateTime approvedDueDate)
        {
            var request = PendingRequest();
            request.Status = LoanRequestStatus.Approved;
            request.ApprovedDueDate = approvedDueDate;
            request.Loan.Status = LoanStatus.Active;
            return request;
        }

        [Fact]
        public async Task RecordRepayment_OnTime_AddsPointsAndMarksLoanReturned()
        {
            var request = ApprovedRequest(DateTime.UtcNow.AddDays(10));

            var result = await _service.RecordRepaymentAsync(5, 99, new RecordRepaymentDto());

            Assert.Equal("Completed", result.Status);
            Assert.Equal(LoanStatus.AwaitingRenewal, request.Loan.Status);
            Assert.True(request.Repayment!.WasOnTime);
            Assert.Equal(99, request.Repayment.RecordedByUserId);
            Assert.Equal(15, request.User.Score); // 5 + 10
        }

        [Fact]
        public async Task RecordRepayment_Late_SubtractsPointsButNotBelowZero()
        {
            var request = ApprovedRequest(DateTime.UtcNow.AddDays(-1));

            await _service.RecordRepaymentAsync(5, 99, new RecordRepaymentDto());

            Assert.False(request.Repayment!.WasOnTime);
            Assert.Equal(0, request.User.Score); // 5 - 20 מתעגל ל-0
        }

        [Fact]
        public async Task RecordRepayment_RequestNotApproved_ThrowsConflict()
        {
            PendingRequest(); // עדיין Pending

            await Assert.ThrowsAsync<ConflictException>(() =>
                _service.RecordRepaymentAsync(5, 99, new RecordRepaymentDto()));
        }
    }
}
