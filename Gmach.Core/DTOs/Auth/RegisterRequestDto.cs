using System.ComponentModel.DataAnnotations;
using Gmach.Core.Validation;

namespace Gmach.Core.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "שם פרטי הוא שדה חובה.")]
        [StringLength(50, ErrorMessage = "שם פרטי יכול להכיל עד 50 תווים.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "שם משפחה הוא שדה חובה.")]
        [StringLength(50, ErrorMessage = "שם משפחה יכול להכיל עד 50 תווים.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "אימייל הוא שדה חובה.")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה.")]
        [StringLength(256, ErrorMessage = "האימייל יכול להכיל עד 256 תווים.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "מספר טלפון הוא שדה חובה.")]
        [RegularExpression(ValidationRules.PhonePattern,
            ErrorMessage = ValidationRules.PhoneMessage)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "סיסמה היא שדה חובה.")]
        [RegularExpression(ValidationRules.PasswordPattern,
            ErrorMessage = ValidationRules.PasswordMessage)]
        public string Password { get; set; } = string.Empty;
    }
}
