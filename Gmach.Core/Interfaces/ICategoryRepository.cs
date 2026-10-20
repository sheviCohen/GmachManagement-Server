using Gmach.Core.Entities;

namespace Gmach.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<LoanCategory>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<LoanCategory?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        // שליפה של כמה קטגוריות בשאילתה אחת (לא בלולאה)
        Task<List<LoanCategory>> GetByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(
            string name,
            CancellationToken cancellationToken = default);

        Task<bool> HasLoansAsync(
            int categoryId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            LoanCategory category,
            CancellationToken cancellationToken = default);

        void Delete(LoanCategory category);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
