using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        Task AddAsync(
            LoanRequest request,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
