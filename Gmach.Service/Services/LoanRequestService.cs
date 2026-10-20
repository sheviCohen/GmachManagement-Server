using AutoMapper;
using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.LoanRequests;
using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;
using Gmach.Core.Interfaces;
using Gmach.Core.Settings;

namespace Gmach.Service.Services
{
    public class LoanRequestService : ILoanRequestService
    {
        private readonly ILoanRequestRepository _requestRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;
        private readonly ScoringSettings _scoring;

        public LoanRequestService(
            ILoanRequestRepository requestRepository,
            ILoanRepository loanRepository,
            IMapper mapper,
            ScoringSettings scoring)
        {
            _requestRepository = requestRepository;
            _loanRepository = loanRepository;
            _mapper = mapper;
            _scoring = scoring;
        }

        public async Task<LoanRequestResponseDto> SubmitAsync(
            int userId,
            SubmitLoanRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // הלוואה מתחילה כ-tracked, כי נשנה את המצב שלה
            var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken)
                ?? throw new NotFoundException("ההלוואה לא נמצאה.");

            if (await _requestRepository.HasOpenRequestAsync(userId, cancellationToken))
                throw new ConflictException(
                    "יש לך כבר בקשה ממתינה או הלוואה פעילה.");

            if (loan.Status != LoanStatus.Available)
                throw new ConflictException("ההלוואה כבר אינה זמינה.");

            loan.Reserve();

            var loanRequest = new LoanRequest
            {
                UserId = userId,
                Loan = loan,
                RequestedAmount = loan.Amount,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                RequestDate = DateTime.UtcNow
            };

            await _requestRepository.AddAsync(loanRequest, cancellationToken);

            // שני לקוחות שהגישו במקביל: אחד יצליח, השני יקבל ConflictException (409)
            await _requestRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanRequestResponseDto>(loanRequest);
        }

        public async Task<LoanRequestResponseDto> CancelAsync(
            int requestId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var loanRequest = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
                ?? throw new NotFoundException("הבקשה לא נמצאה.");

            if (loanRequest.UserId != userId)
                throw new ForbiddenException("אין לך הרשאה לבטל את הבקשה הזו.");

            if (loanRequest.Status != LoanRequestStatus.Pending)
                throw new ConflictException("אפשר לבטל רק בקשה ממתינה.");

            loanRequest.Cancel(DateTime.UtcNow);
            loanRequest.Loan.Release();

            await _requestRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanRequestResponseDto>(loanRequest);
        }

        public async Task<LoanRequestResponseDto> ApproveAsync(
            int requestId,
            CancellationToken cancellationToken = default)
        {
            var loanRequest = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
                ?? throw new NotFoundException("הבקשה לא נמצאה.");

            if (loanRequest.Status != LoanRequestStatus.Pending)
                throw new ConflictException("אפשר לאשר רק בקשה ממתינה.");

            var now = DateTime.UtcNow;

            // תאריך ההחזר נשמר בבקשה כצילום, ונשאר גם כשההלוואה תחזור למאגר
            loanRequest.Approve(loanRequest.Loan.DueDate, now);
            loanRequest.Loan.Activate();

            await _requestRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanRequestResponseDto>(loanRequest);
        }

        public async Task<LoanRequestResponseDto> RejectAsync(
            int requestId,
            RejectLoanRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var reason = request.Reason.Trim();
            if (reason.Length < 2)
                throw new BusinessValidationException("יש לציין סיבת דחייה.");

            var loanRequest = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
                ?? throw new NotFoundException("הבקשה לא נמצאה.");

            if (loanRequest.Status != LoanRequestStatus.Pending)
                throw new ConflictException("אפשר לדחות רק בקשה ממתינה.");

            loanRequest.Reject(reason, DateTime.UtcNow);
            loanRequest.Loan.Release();

            await _requestRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanRequestResponseDto>(loanRequest);
        }

        public async Task<LoanRequestResponseDto> RecordRepaymentAsync(
            int requestId,
            int adminId,
            RecordRepaymentDto request,
            CancellationToken cancellationToken = default)
        {
            var loanRequest = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
                ?? throw new NotFoundException("הבקשה לא נמצאה.");

            if (loanRequest.Status != LoanRequestStatus.Approved)
                throw new ConflictException("אפשר לרשום החזר רק על בקשה מאושרת שטרם הושלמה.");

            var now = DateTime.UtcNow;
            var received = NormalizeToUtc(request.ReceivedDate ?? now);

            if (received > now.AddMinutes(5))
                throw new BusinessValidationException("תאריך קבלת הכסף לא יכול להיות בעתיד.");

            // ApprovedDueDate נשמר באישור, ולכן הוא לא מושפע משינויים מאוחרים בהלוואה
            var dueDate = loanRequest.ApprovedDueDate
                ?? throw new InvalidOperationException("Approved request has no due date.");
            var wasOnTime = received <= dueDate;

            // Repayment חדש נשמר דרך ה-navigation property, באותה שמירה
            loanRequest.Repayment = new Repayment
            {
                ReceivedDate = received,
                WasOnTime = wasOnTime,
                RecordedByUserId = adminId
            };

            loanRequest.Complete();
            loanRequest.Loan.MarkAsReturned();
            loanRequest.User.ChangeScore(
                wasOnTime ? _scoring.OnTimeReturnPoints : -_scoring.LateReturnPenalty);

            await _requestRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LoanRequestResponseDto>(loanRequest);
        }

        private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        public async Task<PagedResultDto<AdminLoanRequestResponseDto>> GetPagedAsync(
            LoanRequestQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _requestRepository.GetPagedAsync(
                query, cancellationToken);

            return new PagedResultDto<AdminLoanRequestResponseDto>
            {
                Items = _mapper.Map<List<AdminLoanRequestResponseDto>>(items),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<LoanRequestResponseDto>> GetMineAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var requests = await _requestRepository.GetByUserIdAsync(userId, cancellationToken);
            return _mapper.Map<List<LoanRequestResponseDto>>(requests);
        }

        public async Task<LoanRequestResponseDto> GetByIdAsync(
            int requestId,
            int requesterId,
            UserRole requesterRole,
            CancellationToken cancellationToken = default)
        {
            var loanRequest = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
                ?? throw new NotFoundException("הבקשה לא נמצאה.");

            if (requesterRole != UserRole.Admin && loanRequest.UserId != requesterId)
                throw new ForbiddenException("אין לך הרשאה לצפות בבקשה הזו.");

            return _mapper.Map<LoanRequestResponseDto>(loanRequest);
        }
    }
}
