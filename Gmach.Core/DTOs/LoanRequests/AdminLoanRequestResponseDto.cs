namespace Gmach.Core.DTOs.LoanRequests
{
    // בקשה כפי שהמנהל רואה אותה: כולל פרטי הלקוח וציון האמינות שלו
    public class AdminLoanRequestResponseDto : LoanRequestResponseDto
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int UserScore { get; set; }
    }
}
