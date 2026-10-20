using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Interfaces;

namespace Gmach.Service.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;

        public LoanService(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<Loan> CreateAsync(
            decimal amount,
            DateTime dueDate,
            CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "Loan amount must be positive.");

            if (dueDate <= DateTime.UtcNow)
                throw new ArgumentException(
                    "Due date must be in the future.");

            var loan = new Loan
            {
                Amount = amount,
                DueDate = dueDate,
                Status = LoanStatus.Available,
                Version = Guid.NewGuid()
            };

            await _loanRepository.AddAsync(
                loan, cancellationToken);

            await _loanRepository.SaveChangesAsync(
                cancellationToken);

            return loan;
        }

        public async Task<List<Loan>> GetAvailableAsync(
            CancellationToken cancellationToken = default)
        {
            return await _loanRepository.GetAvailableAsync(
                cancellationToken);
        }

        public async Task<Loan?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentException(
                    "Invalid loan ID.");

            return await _loanRepository.GetByIdAsync(
                id, cancellationToken);
        }
    }
}
