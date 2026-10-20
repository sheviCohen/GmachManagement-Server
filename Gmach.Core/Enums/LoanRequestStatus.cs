namespace Gmach.Core.Enums
{
    public enum LoanRequestStatus
    {
        Pending,    // ממתינה להחלטת המנהל
        Approved,   // אושרה - ההלוואה פעילה
        Rejected,   // נדחתה
        Cancelled,  // בוטלה על ידי הלקוח
        Completed   // ההלוואה הוחזרה והבקשה הסתיימה
    }
}
