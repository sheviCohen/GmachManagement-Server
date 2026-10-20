using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.LoanRequests
{
    // הגשת בקשה להלוואה מהמאגר
    public class SubmitLoanRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "יש לבחור הלוואה.")]
        public int LoanId { get; set; }

        [StringLength(500, ErrorMessage = "ההערה יכולה להכיל עד 500 תווים.")]
        public string? Note { get; set; }
    }
}
