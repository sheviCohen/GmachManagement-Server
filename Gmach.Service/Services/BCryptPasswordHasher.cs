using Gmach.Core.Interfaces;

namespace Gmach.Service.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            // BCrypt מוסיף "מלח" אקראי לכל גיבוב ושומר אותו בתוך התוצאה
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
