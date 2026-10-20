using Gmach.Core.Entities;

namespace Gmach.Core.Interfaces
{
    public interface IJwtTokenGenerator
    {
        // יוצר token חתום למשתמש ומחזיר גם את זמן התפוגה שלו (UTC)
        (string Token, DateTime ExpiresAt) Generate(User user);
    }
}
