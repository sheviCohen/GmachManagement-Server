using System.ComponentModel.DataAnnotations;
using Gmach.Core.Enums;

namespace Gmach.Core.DTOs.Loans
{
    // פרמטרים של רשימת ההלוואות: דפדוף, סינון ומיון. מגיעים מה-query string.
    public class LoanQueryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מספר העמוד חייב להיות 1 או יותר.")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "גודל העמוד חייב להיות בין 1 ל-50.")]
        public int PageSize { get; set; } = 10;

        public int? CategoryId { get; set; }

        [Range(typeof(decimal), "0", "1000000000",
            ErrorMessage = "הסכום המינימלי אינו תקין.")]
        public decimal? MinAmount { get; set; }

        [Range(typeof(decimal), "0", "1000000000",
            ErrorMessage = "הסכום המקסימלי אינו תקין.")]
        public decimal? MaxAmount { get; set; }

        // סינון לפי מצב. רלוונטי למנהל בלבד; לקוח תמיד רואה הלוואות זמינות בלבד.
        public LoanStatus? Status { get; set; }

        public LoanSortBy SortBy { get; set; } = LoanSortBy.Amount;

        public bool Descending { get; set; } = false;
    }
}
