using System.ComponentModel.DataAnnotations;
using Gmach.Core.Enums;

namespace Gmach.Core.DTOs.LoanRequests
{
    // דפדוף וסינון של רשימת הבקשות למנהל (מה-query string)
    public class LoanRequestQueryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מספר העמוד חייב להיות 1 או יותר.")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "גודל העמוד חייב להיות בין 1 ל-50.")]
        public int PageSize { get; set; } = 10;

        // למשל Pending כדי לראות רק בקשות שממתינות להחלטה
        public LoanRequestStatus? Status { get; set; }
    }
}
