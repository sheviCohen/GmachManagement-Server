using Gmach.Core.Enums;

namespace Gmach.Core.Entities
{
    // פריט במאגר ההלוואות - זה המשאב המוגבל שעליו מתחרים.
    public class Loan
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Available;

        // Concurrency token: מתחלף בכל עדכון (ב-GmachDbContext.SaveChangesAsync).
        public Guid Version { get; set; } = Guid.NewGuid();

        public ICollection<LoanCategory> Categories { get; set; }
            = new List<LoanCategory>();

        public ICollection<LoanRequest> Requests { get; set; }
            = new List<LoanRequest>();

        // זמינה -> תפוסה (הוגשה בקשה)
        public void Reserve()
        {
            if (Status != LoanStatus.Available)
                throw new InvalidOperationException("Loan is not available.");
            Status = LoanStatus.Reserved;
        }

        // תפוסה -> זמינה (בקשה נדחתה או בוטלה)
        public void Release()
        {
            if (Status != LoanStatus.Reserved)
                throw new InvalidOperationException("Loan is not reserved.");
            Status = LoanStatus.Available;
        }

        // תפוסה -> פעילה (בקשה אושרה)
        public void Activate()
        {
            if (Status != LoanStatus.Reserved)
                throw new InvalidOperationException("Loan is not reserved.");
            Status = LoanStatus.Active;
        }

        // פעילה -> ממתינה לתאריך חדש (הכסף חזר)
        public void MarkAsReturned()
        {
            if (Status != LoanStatus.Active)
                throw new InvalidOperationException("Only active loans can be returned.");
            Status = LoanStatus.AwaitingRenewal;
        }

        // ממתינה לתאריך חדש -> זמינה (המנהל קבע תאריך חדש)
        public void Renew(DateTime newDueDate)
        {
            if (Status != LoanStatus.AwaitingRenewal)
                throw new InvalidOperationException("Loan is not awaiting renewal.");
            if (newDueDate <= DateTime.UtcNow)
                throw new ArgumentException("New due date must be in the future.");

            DueDate = newDueDate;
            Status = LoanStatus.Available;
        }
    }
}
