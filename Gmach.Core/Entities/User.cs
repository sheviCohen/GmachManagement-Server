using Gmach.Core.Enums;

namespace Gmach.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;

        // ציון אמינות - גלוי למנהל בלבד. ערך גבוה = לקוח אמין יותר.
        public int Score { get; set; } = 0;

        public ICollection<LoanRequest> LoanRequests { get; set; }
            = new List<LoanRequest>();

        // שינוי הציון (חיובי = שיפור, שלילי = החמרה). הציון לא יורד מתחת ל-0.
        public void ChangeScore(int delta)
        {
            Score = Math.Max(0, checked(Score + delta));
        }
    }
}
