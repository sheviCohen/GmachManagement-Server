using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.LoanRequests
{
    // דחיית בקשה על ידי המנהל. הסיבה חובה והלקוח רואה אותה.
    public class RejectLoanRequestDto
    {
        [Required(ErrorMessage = "יש לציין סיבת דחייה.")]
        [StringLength(500, MinimumLength = 2,
            ErrorMessage = "סיבת הדחייה חייבת להכיל בין 2 ל-500 תווים.")]
        public string Reason { get; set; } = string.Empty;
    }
}
