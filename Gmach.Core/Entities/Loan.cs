using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gmach.Core.Enums;

using Gmach.Core.Enums;
using Gmach.Core.Enums;

namespace Gmach.Core.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public LoanStatus Status { get; set; }
            = LoanStatus.Available;

        public int? BorrowerId { get; set; }
        public User? Borrower { get; set; }

        public Guid Version { get; set; }
            = Guid.NewGuid();

        public void AssignToUser(int userId)
        {
            if (Status != LoanStatus.Available)
                throw new InvalidOperationException(
                    "Loan is not available");

            if (userId <= 0)
                throw new ArgumentException(
                    "Invalid user ID");

            BorrowerId = userId;
            Status = LoanStatus.Active;
        }
    }
}