using System.Security.Claims;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;

namespace Gmach.API.Extensions
{
    // קריאת פרטי המשתמש המחובר מתוך ה-claims של ה-token
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue("sub");

            if (!int.TryParse(value, out var id))
                throw new UnauthorizedException("ה-token אינו תקין.");

            return id;
        }

        public static UserRole GetRole(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.Role);

            if (!Enum.TryParse<UserRole>(value, out var role))
                throw new UnauthorizedException("ה-token אינו תקין.");

            return role;
        }
    }
}
