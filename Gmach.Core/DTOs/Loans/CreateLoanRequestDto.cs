using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.Loans
{
    public class CreateLoanRequestDto
    {
        [Range(typeof(decimal), "0.01", "1000000000",
            ErrorMessage = "הסכום חייב להיות חיובי.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "תאריך ההחזר הוא שדה חובה.")]
        public DateTime? DueDate { get; set; }

        // מזהי הקטגוריות של ההלוואה (אופציונלי)
        public List<int> CategoryIds { get; set; } = new();
    }
}
