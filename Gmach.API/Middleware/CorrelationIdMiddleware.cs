using System.Text.RegularExpressions;

namespace Gmach.API.Middleware
{
    // נותן לכל בקשה מזהה אחד (CorrelationId): מהכותרת X-Correlation-ID אם הלקוח שלח כזו תקינה,
    // אחרת מזהה חדש. המזהה נכנס לכל שורות הלוג של הבקשה, לתשובת השגיאה ולכותרת התשובה.
    public partial class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = ResolveCorrelationId(context);

            // ErrorResponse.TraceId נלקח מ-TraceIdentifier, ולכן הוא יזהה את הבקשה בדיוק כמו הלוג
            context.TraceIdentifier = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            // כל הלוגים שנכתבים בתוך הבלוק כוללים את CorrelationId
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                await _next(context);
            }
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            var incoming = context.Request.Headers[HeaderName].ToString();

            // מקבלים רק ערך קצר ובטוח, כדי שאי אפשר יהיה להחדיר תווים מוזרים לקובץ הלוג
            if (!string.IsNullOrWhiteSpace(incoming) && SafeId().IsMatch(incoming))
                return incoming;

            return Guid.NewGuid().ToString("N");
        }

        [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$")]
        private static partial Regex SafeId();
    }
}
