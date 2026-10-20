using System.Text.Json;
using Gmach.API.Models;

namespace Gmach.API.Middleware
{
    // כותב תשובת שגיאה אחידה בפורמט JSON. משמש גם את ה-middleware וגם את JWT.
    public static class ErrorResponseWriter
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        public static Task WriteAsync(
            HttpContext context,
            int statusCode,
            string message)
        {
            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(Create(context, statusCode, message), JsonOptions));
        }

        public static ErrorResponse Create(
            HttpContext context,
            int statusCode,
            string message,
            IDictionary<string, string[]>? errors = null)
        {
            return new ErrorResponse
            {
                Status = statusCode,
                Message = message,
                TraceId = context.TraceIdentifier,
                Errors = errors
            };
        }
    }
}
