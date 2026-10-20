using Gmach.Core.Enums;

namespace Gmach.Core.Entities
{
    public class LoanRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int LoanId { get; set; }
        public Loan Loan { get; set; } = null!;

        public Repayment? Repayment { get; set; }

        // צילום של הסכום ברגע ההגשה
        public decimal RequestedAmount { get; set; }

        // הערה של הלקוח למנהל (אופציונלית)
        public string? Note { get; set; }

        public LoanRequestStatus Status { get; set; } = LoanRequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        // מתי המנהל אישר/דחה, או מתי הלקוח ביטל
        public DateTime? DecisionDate { get; set; }

        // חובה כשהמנהל דוחה, והלקוח רואה אותה
        public string? RejectionReason { get; set; }

        // צילום של תאריך ההחזר ברגע האישור - נשמר גם אחרי שההלוואה חוזרת למאגר
        public DateTime? ApprovedDueDate { get; set; }

        public void Approve(DateTime dueDate, DateTime now)
        {
            EnsurePending();
            Status = LoanRequestStatus.Approved;
            ApprovedDueDate = dueDate;
            DecisionDate = now;
        }

        public void Reject(string reason, DateTime now)
        {
            EnsurePending();
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required.");
            Status = LoanRequestStatus.Rejected;
            RejectionReason = reason;
            DecisionDate = now;
        }

        public void Cancel(DateTime now)
        {
            EnsurePending();
            Status = LoanRequestStatus.Cancelled;
            DecisionDate = now;
        }

        public void Complete()
        {
            if (Status != LoanRequestStatus.Approved)
                throw new InvalidOperationException("Only approved requests can be completed.");
            Status = LoanRequestStatus.Completed;
        }

        private void EnsurePending()
        {
            if (Status != LoanRequestStatus.Pending)
                throw new InvalidOperationException("Request is not pending.");
        }
    }
}
