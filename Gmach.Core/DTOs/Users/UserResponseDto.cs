namespace Gmach.Core.DTOs.Users
{
    // מה שמוחזר ללקוח על משתמש. בכוונה בלי PasswordHash ובלי Score.
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
