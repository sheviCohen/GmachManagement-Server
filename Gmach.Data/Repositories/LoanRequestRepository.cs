using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gmach.Core.Entities;
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

        public async Task AddAsync(
            LoanRequest request,
            CancellationToken cancellationToken = default)
        {
            await _context.LoanRequests.AddAsync(request, cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
