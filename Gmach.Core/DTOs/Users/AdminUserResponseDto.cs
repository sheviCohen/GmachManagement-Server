namespace Gmach.Core.DTOs.Users
{
    // לקוח כפי שהמנהל רואה אותו: כולל ציון אמינות. עדיין בלי סיסמה או גיבוב.
    public class AdminUserResponseDto : UserResponseDto
    {
        public int Score { get; set; }
    }
}
