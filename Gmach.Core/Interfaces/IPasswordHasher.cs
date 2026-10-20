namespace Gmach.Core.Interfaces
{
    public interface IPasswordHasher
    {
        // יוצר גיבוב (hash) מסיסמה. לא ניתן לשחזר ממנו את הסיסמה.
        string Hash(string password);

        // בודק אם סיסמה תואמת לגיבוב שנשמר.
        bool Verify(string password, string passwordHash);
    }
}
