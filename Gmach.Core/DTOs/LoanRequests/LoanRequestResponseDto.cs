namespace Gmach.Core.DTOs.LoanRequests
{
    public class LoanRequestResponseDto
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public decimal RequestedAmount { get; set; }
        public DateTime LoanDueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DecisionDate { get; set; }

        // סיבת הדחייה - מוצגת ללקוח
        public string? RejectionReason { get; set; }
        public DateTime? ApprovedDueDate { get; set; }
    }
}
