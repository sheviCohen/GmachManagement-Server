namespace Gmach.Core.Settings
{
    // הגדרות נתוני ההתחלה. נקראות מהקונפיגורציה (סעיף "Seed").
    // הסיסמאות לא נכתבות בקוד: בפיתוח ב-User Secrets, בפריסה במשתני סביבה.
    public class SeedSettings
    {
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
    }
}
