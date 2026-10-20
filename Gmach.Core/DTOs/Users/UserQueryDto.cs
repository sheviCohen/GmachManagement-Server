using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.Users
{
    public class UserQueryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מספר העמוד חייב להיות 1 או יותר.")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "גודל העמוד חייב להיות בין 1 ל-50.")]
        public int PageSize { get; set; } = 10;
    }
}
