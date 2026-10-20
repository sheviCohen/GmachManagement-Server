using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gmach.Core.DTOs.LoanRequests;
using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;
using Gmach.Core.Interfaces;
using Gmach.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gmach.Data.Repositories
{
    public class LoanRequestRepository : ILoanRequestRepository
    {
        private readonly GmachDbContext _context;

        public LoanRequestRepository(GmachDbContext context)
        {
            _context = context;
        }

        public async Task<LoanRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoanRequests
                .Include(r => r.Loan)
                .Include(r => r.User)
                .Include(r => r.Repayment)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<List<LoanRequest>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoanRequests
                .AsNoTracking()
                .Include(r => r.Loan)
                .Include(r => r.Repayment)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<LoanRequest> Items, int TotalCount)> GetPagedAsync(
            LoanRequestQueryDto query,
            CancellationToken cancellationToken = default)
        {
            IQueryable<LoanRequest> requests = _context.LoanRequests.AsNoTracking();

            if (query.Status.HasValue)
                requests = requests.Where(r => r.Status == query.Status.Value);

            var totalCount = await requests.CountAsync(cancellationToken);

            // Include של User ו-Loan באותה שאילתה: בלי N+1
            var items = await requests
                .OrderByDescending(r => r.RequestDate)
                .ThenBy(r => r.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Include(r => r.Loan)
                .Include(r => r.User)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> HasOpenRequestAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.LoanRequests
                .AnyAsync(r => r.UserId == userId &&
                    (r.Status == LoanRequestStatus.Pending ||
                     r.Status == LoanRequestStatus.Approved),
                    cancellationToken);
        }

        public async Task AddAsync(
            LoanRequest request,
            CancellationToken cancellationToken = default)
        {
            await _context.LoanRequests.AddAsync(request, cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // מישהו אחר שינה את אותה שורה בין השליפה לשמירה
                throw new ConflictException(
                    "הנתונים השתנו בינתיים על ידי משתמש אחר. רעננו ונסו שוב.");
            }
        }
    }
}
