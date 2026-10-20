using Gmach.Core.DTOs.Common;
using Gmach.Core.DTOs.Loans;
using Gmach.Core.Enums;

namespace Gmach.Core.Interfaces
{
    public interface ILoanService
    {
        // לקוח רואה הלוואות זמינות בלבד; מנהל יכול לסנן לפי מצב
        Task<PagedResultDto<LoanResponseDto>> GetPagedAsync(
            LoanQueryDto query,
            UserRole requesterRole,
            CancellationToken cancellationToken = default);

        Task<LoanResponseDto> GetByIdAsync(
            int id,
            UserRole requesterRole,
            CancellationToken cancellationToken = default);

        Task<LoanResponseDto> CreateAsync(
            CreateLoanRequestDto request,
            CancellationToken cancellationToken = default);

        Task<LoanResponseDto> UpdateAsync(
            int id,
            UpdateLoanRequestDto request,
            CancellationToken cancellationToken = default);

        // קביעת תאריך חדש להלוואה שחזרה: AwaitingRenewal -> Available
        Task<LoanResponseDto> RenewAsync(
            int id,
            RenewLoanDto request,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
