using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "אימייל הוא שדה חובה.")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה.")]
        public string Email { get; set; } = string.Empty;

        // בכוונה בלי בדיקת תבנית: לא חושפים בהתחברות מהם כללי הסיסמה
        [Required(ErrorMessage = "סיסמה היא שדה חובה.")]
        public string Password { get; set; } = string.Empty;
    }
}
