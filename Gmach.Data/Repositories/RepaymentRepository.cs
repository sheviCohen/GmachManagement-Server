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
    public class RepaymentRepository : IRepaymentRepository
    {
        private readonly GmachDbContext _context;

        public RepaymentRepository(GmachDbContext context)
        {
            _context = context;
        }

        public async Task<Repayment?> GetByLoanRequestIdAsync(
            int loanRequestId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Repayments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.LoanRequestId == loanRequestId,
                    cancellationToken);
        }

        public async Task AddAsync(
            Repayment repayment,
            CancellationToken cancellationToken = default)
        {
            await _context.Repayments.AddAsync(
                repayment, cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
