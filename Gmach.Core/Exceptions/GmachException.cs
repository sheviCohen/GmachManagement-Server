namespace Gmach.Core.Exceptions
{
    // בסיס לכל השגיאות העסקיות. ה-middleware הגלובלי ממיר אותן לתשובות HTTP.
    public abstract class GmachException : Exception
    {
        protected GmachException(string message) : base(message) { }
    }

    // 400 - הבקשה תקינה מבחינת מבנה, אבל מפרה כלל עסקי
    public class BusinessValidationException : GmachException
    {
        public BusinessValidationException(string message) : base(message) { }
    }

    // 401 - לא מחובר, או פרטי התחברות שגויים
    public class UnauthorizedException : GmachException
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    // 403 - מחובר, אבל אין הרשאה לפעולה הזאת
    public class ForbiddenException : GmachException
    {
        public ForbiddenException(string message) : base(message) { }
    }

    // 404 - המשאב לא נמצא
    public class NotFoundException : GmachException
    {
        public NotFoundException(string message) : base(message) { }
    }

    // 409 - הבקשה מתנגשת עם המצב הנוכחי (אימייל קיים, הלוואה כבר נתפסה...)
    public class ConflictException : GmachException
    {
        public ConflictException(string message) : base(message) { }
    }
}
