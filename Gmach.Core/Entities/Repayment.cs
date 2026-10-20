namespace Gmach.Core.Entities
{
    // החזר מלא שנרשם על ידי המנהל.
    public class Repayment
    {
        public int Id { get; set; }

        public int LoanRequestId { get; set; }
        public LoanRequest LoanRequest { get; set; } = null!;

        // התאריך שבו המנהל קיבל את הכסף בפועל
        public DateTime ReceivedDate { get; set; }

        // האם ההחזר היה עד תאריך ההחזר שנקבע
        public bool WasOnTime { get; set; }

        public int RecordedByUserId { get; set; }
        public User RecordedByUser { get; set; } = null!;
    }
}
