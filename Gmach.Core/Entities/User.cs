using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gmach.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public int Status { get; set; } = 0;

        public ICollection<Loan> Loans { get; set; }
            = new List<Loan>();

        public void IncreaseStatus(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");

            Status = checked(Status + amount);
        }
    }
}