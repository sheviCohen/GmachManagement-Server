using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.Loans
{
    // קביעת תאריך החזר חדש להלוואה שהכסף שלה חזר למאגר
    public class RenewLoanDto
    {
        [Required(ErrorMessage = "תאריך ההחזר הוא שדה חובה.")]
        public DateTime? DueDate { get; set; }
    }
}
