using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gmach.Core.Entities;

namespace Gmach.Core.Interfaces
{
    public interface ILoanService
    {
        Task<Loan> CreateAsync(
            decimal amount,
            DateTime dueDate,
            CancellationToken cancellationToken = default);

        Task<List<Loan>> GetAvailableAsync(
            CancellationToken cancellationToken = default);

        Task<Loan?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
