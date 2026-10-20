using Gmach.Core.DTOs.Loans;
using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Interfaces;
using Gmach.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gmach.Data.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly GmachDbContext _context;

        public LoanRepository(GmachDbContext context)
        {
            _context = context;
        }

        public async Task<Loan?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public async Task<Loan?> GetByIdWithCategoriesAsync(
            int id,
            bool track,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Loan> query = _context.Loans
                .Include(l => l.Categories);

            if (!track)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(
                l => l.Id == id, cancellationToken);
        }

        public async Task<List<Loan>> GetAvailableAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Loans
                .AsNoTracking()
                .Where(l => l.Status == LoanStatus.Available)
                .OrderBy(l => l.Amount)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Loan> Items, int TotalCount)> GetPagedAsync(
            LoanQueryDto query,
            LoanStatus? status,
            CancellationToken cancellationToken = default)
        {
            // IQueryable: כאן רק בונים שאילתה. היא תרוץ בבסיס הנתונים רק ב-CountAsync/ToListAsync.
            IQueryable<Loan> loans = _context.Loans.AsNoTracking();

            if (status.HasValue)
                loans = loans.Where(l => l.Status == status.Value);

            if (query.CategoryId.HasValue)
                loans = loans.Where(l =>
                    l.Categories.Any(c => c.Id == query.CategoryId.Value));

            if (query.MinAmount.HasValue)
                loans = loans.Where(l => l.Amount >= query.MinAmount.Value);

            if (query.MaxAmount.HasValue)
                loans = loans.Where(l => l.Amount <= query.MaxAmount.Value);

            // הספירה נעשית אחרי הסינון ולפני הדפדוף
            var totalCount = await loans.CountAsync(cancellationToken);

            // מיון, ובנוסף לפי Id כדי שהסדר בין עמודים יהיה יציב
            IOrderedQueryable<Loan> ordered = query.SortBy switch
            {
                LoanSortBy.DueDate => query.Descending
                    ? loans.OrderByDescending(l => l.DueDate)
                    : loans.OrderBy(l => l.DueDate),
                _ => query.Descending
                    ? loans.OrderByDescending(l => l.Amount)
                    : loans.OrderBy(l => l.Amount)
            };

            // Skip ו-Take בתוך השאילתה, וה-Include טוען את הקטגוריות בלי N+1
            var items = await ordered
                .ThenBy(l => l.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Include(l => l.Categories)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> HasRequestsAsync(
            int loanId,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoanRequests
                .AnyAsync(r => r.LoanId == loanId, cancellationToken);
        }

        public async Task AddAsync(
            Loan loan,
            CancellationToken cancellationToken = default)
        {
            await _context.Loans.AddAsync(loan, cancellationToken);
        }

        public void Delete(Loan loan)
        {
            _context.Loans.Remove(loan);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
