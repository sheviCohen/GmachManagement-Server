using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.Loans
{
    public class UpdateLoanRequestDto
    {
        [Range(typeof(decimal), "0.01", "1000000000",
            ErrorMessage = "הסכום חייב להיות חיובי.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "תאריך ההחזר הוא שדה חובה.")]
        public DateTime? DueDate { get; set; }

        public List<int> CategoryIds { get; set; } = new();
    }
}
