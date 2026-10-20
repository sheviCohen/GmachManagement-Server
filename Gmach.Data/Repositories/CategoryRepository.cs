using Gmach.Core.Entities;
using Gmach.Core.Interfaces;
using Gmach.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gmach.Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly GmachDbContext _context;

        public CategoryRepository(GmachDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoanCategory>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.LoanCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<LoanCategory?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoanCategories
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<List<LoanCategory>> GetByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            var idList = ids.Distinct().ToList();

            return await _context.LoanCategories
                .Where(c => idList.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoanCategories
                .AnyAsync(c => c.Name == name, cancellationToken);
        }

        public async Task<bool> HasLoansAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Loans
                .AnyAsync(l => l.Categories.Any(c => c.Id == categoryId),
                    cancellationToken);
        }

        public async Task AddAsync(
            LoanCategory category,
            CancellationToken cancellationToken = default)
        {
            await _context.LoanCategories.AddAsync(category, cancellationToken);
        }

        public void Delete(LoanCategory category)
        {
            _context.LoanCategories.Remove(category);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
