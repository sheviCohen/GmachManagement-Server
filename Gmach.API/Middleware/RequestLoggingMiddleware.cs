using System.Diagnostics;

namespace Gmach.API.Middleware
{
    // כותב שורת לוג אחת לכל בקשה: שיטה, כתובת, קוד תשובה וזמן.
    // בכוונה לא נכתבים גוף הבקשה, כותרות או query string, כי הם עלולים להכיל סיסמאות וטוקנים.
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var failed = false;

            try
            {
                await _next(context);
            }
            catch
            {
                // הטיפול בחריגה (והלוג שלה) נעשה ב-ExceptionHandlingMiddleware שלפנינו
                failed = true;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                if (failed)
                    _logger.LogInformation(
                        "HTTP {Method} {Path} failed after {ElapsedMs} ms",
                        context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
                else
                    _logger.LogInformation(
                        "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                        context.Request.Method, context.Request.Path,
                        context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
