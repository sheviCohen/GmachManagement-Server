using Gmach.Core.Exceptions;

namespace Gmach.API.Middleware
{
    // תופס כל חריגה שלא טופלה ומחזיר תשובת JSON אחידה.
    // חייב להיות הראשון ב-pipeline, כדי לתפוס שגיאות מכל מה שאחריו.
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (GmachException ex)
            {
                using var scope = BeginCorrelationScope(context);

                var statusCode = ex switch
                {
                    BusinessValidationException => StatusCodes.Status400BadRequest,
                    UnauthorizedException => StatusCodes.Status401Unauthorized,
                    ForbiddenException => StatusCodes.Status403Forbidden,
                    NotFoundException => StatusCodes.Status404NotFound,
                    ConflictException => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };

                // התנגשות על משאב היא אירוע שמעניין לראות בלוג (Warning), השאר ברמת Information
                if (ex is ConflictException)
                    _logger.LogWarning("Conflict: {Message}", ex.Message);
                else
                    _logger.LogInformation("Business error {Status}: {Message}", statusCode, ex.Message);

                await ErrorResponseWriter.WriteAsync(context, statusCode, ex.Message);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // הלקוח ביטל את הבקשה. אין למי להחזיר תשובה, וזו לא שגיאה.
                _logger.LogDebug("Request was cancelled by the client.");
            }
            catch (Exception ex)
            {
                using var scope = BeginCorrelationScope(context);
                _logger.LogError(ex, "Unhandled exception");

                // בכוונה הודעה כללית: לא חושפים ללקוח פרטים פנימיים
                await ErrorResponseWriter.WriteAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "אירעה שגיאה בשרת. נסי שוב מאוחר יותר.");
            }
        }

        // ה-scope של CorrelationIdMiddleware כבר נסגר כשהחריגה הגיעה לכאן, לכן פותחים אותו שוב
        private IDisposable? BeginCorrelationScope(HttpContext context)
            => _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = context.TraceIdentifier
            });
    }
}
