namespace Gmach.Core.Enums
{
    public enum LoanStatus
    {
        Available,       // זמינה - מוצגת ללקוחות
        Reserved,        // תפוסה - יש עליה בקשה ממתינה
        Active,          // פעילה - אושרה וניתנה ללקוח
        AwaitingRenewal  // הכסף חזר, ממתינה שהמנהל יקבע תאריך החזר חדש
    }
}
