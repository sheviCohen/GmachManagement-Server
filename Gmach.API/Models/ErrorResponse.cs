namespace Gmach.API.Models
{
    // מבנה אחיד לכל תשובות השגיאה של ה-API
    public class ErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;

        // שגיאות ולידציה לפי שדה (רק בתשובות 400 של ולידציית קלט)
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}
