using Gmach.Core.DTOs.Loans;
using Gmach.Core.Entities;
using Gmach.Core.Enums;

namespace Gmach.Core.Interfaces
{
    public interface ILoanRepository
    {
        Task<Loan?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        // שליפה כולל הקטגוריות. track=false לקריאה בלבד, track=true לעדכון/מחיקה.
        Task<Loan?> GetByIdWithCategoriesAsync(
            int id,
            bool track,
            CancellationToken cancellationToken = default);

        // רשימה מדופדפת עם סינון ומיון. כל הפעולות מתבצעות בבסיס הנתונים.
        // status = הסינון בפועל לפי מצב (ה-Service קובע אותו לפי התפקיד).
        Task<(List<Loan> Items, int TotalCount)> GetPagedAsync(
            LoanQueryDto query,
            LoanStatus? status,
            CancellationToken cancellationToken = default);

        Task<bool> HasRequestsAsync(
            int loanId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Loan loan,
            CancellationToken cancellationToken = default);

        void Delete(Loan loan);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
