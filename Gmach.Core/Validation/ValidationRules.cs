namespace Gmach.Core.Validation
{
    // כללי ולידציה משותפים, כדי שלא יוגדרו פעמיים בכמה מקומות.
    public static class ValidationRules
    {
        // לפחות 8 תווים, אות אנגלית קטנה, אות אנגלית גדולה ולפחות שתי ספרות.
        // מותרים רק אותיות אנגליות וספרות.
        public const string PasswordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=(?:.*\d){2,})[A-Za-z\d]{8,64}$";

        public const string PasswordMessage =
            "הסיסמה חייבת להכיל לפחות 8 תווים: אות אנגלית קטנה, אות אנגלית גדולה ולפחות שתי ספרות. מותרים רק אותיות אנגליות וספרות.";

        // מספר טלפון ישראלי: ספרות בלבד, מתחיל ב-0, 9-10 ספרות
        public const string PhonePattern = @"^0\d{8,9}$";

        public const string PhoneMessage =
            "מספר הטלפון חייב להכיל 9-10 ספרות ולהתחיל ב-0.";
    }
}
