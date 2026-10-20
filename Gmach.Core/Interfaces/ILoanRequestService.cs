using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.LoanRequests;
using Gmach.Core.Enums;

namespace Gmach.Core.Interfaces
{
    public interface ILoanRequestService
    {
        Task<LoanRequestResponseDto> SubmitAsync(
            int userId,
            SubmitLoanRequestDto request,
            CancellationToken cancellationToken = default);

        Task<LoanRequestResponseDto> CancelAsync(
            int requestId,
            int userId,
            CancellationToken cancellationToken = default);

        // החלטות מנהל על בקשה ממתינה
        Task<LoanRequestResponseDto> ApproveAsync(
            int requestId,
            CancellationToken cancellationToken = default);

        Task<LoanRequestResponseDto> RejectAsync(
            int requestId,
            RejectLoanRequestDto request,
            CancellationToken cancellationToken = default);

        // רישום החזר: הבקשה מושלמת, ההלוואה ממתינה לתאריך חדש וציון הלקוח משתנה
        Task<LoanRequestResponseDto> RecordRepaymentAsync(
            int requestId,
            int adminId,
            RecordRepaymentDto request,
            CancellationToken cancellationToken = default);

        // רשימת כל הבקשות למנהל, עם דפדוף וסינון לפי מצב
        Task<PagedResultDto<AdminLoanRequestResponseDto>> GetPagedAsync(
            LoanRequestQueryDto query,
            CancellationToken cancellationToken = default);

        Task<List<LoanRequestResponseDto>> GetMineAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<LoanRequestResponseDto> GetByIdAsync(
            int requestId,
            int requesterId,
            UserRole requesterRole,
            CancellationToken cancellationToken = default);
    }
}
