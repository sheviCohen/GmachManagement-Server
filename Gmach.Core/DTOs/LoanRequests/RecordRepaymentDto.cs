namespace Gmach.Core.DTOs.LoanRequests
{
    // רישום החזר על ידי המנהל. אם לא נשלח תאריך, נלקח הרגע הנוכחי.
    public class RecordRepaymentDto
    {
        public DateTime? ReceivedDate { get; set; }
    }
}
