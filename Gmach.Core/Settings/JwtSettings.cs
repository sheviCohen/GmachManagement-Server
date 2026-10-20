namespace Gmach.Core.Settings
{
    // הגדרות ה-JWT (סעיף "Jwt" בקונפיגורציה).
    // Key הוא סוד: בפיתוח ב-User Secrets, בפריסה במשתנה סביבה Jwt__Key.
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiresMinutes { get; set; } = 15;
    }
}
