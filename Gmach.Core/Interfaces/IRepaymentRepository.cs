using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gmach.Core.Entities;

namespace Gmach.Core.Interfaces
{
    public interface IRepaymentRepository
    {
        Task<Repayment?> GetByLoanRequestIdAsync(
            int loanRequestId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Repayment repayment,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
