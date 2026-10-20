using Gmach.Core.DTOs.LoanRequests;
using Gmach.Core.Entities;

namespace Gmach.Core.Interfaces
{
    public interface ILoanRequestRepository
    {
        Task<LoanRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<List<LoanRequest>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        // האם ללקוח יש בקשה ממתינה או הלוואה פעילה (בקשה מאושרת שטרם הושלמה)
        // רשימה מדופדפת למנהל, עם פרטי הלקוח וההלוואה. הסינון והדפדוף בבסיס הנתונים.
        Task<(List<LoanRequest> Items, int TotalCount)> GetPagedAsync(
            LoanRequestQueryDto query,
            CancellationToken cancellationToken = default);

        Task<bool> HasOpenRequestAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            LoanRequest request,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
