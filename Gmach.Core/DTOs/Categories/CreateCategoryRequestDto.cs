using System.ComponentModel.DataAnnotations;

namespace Gmach.Core.DTOs.Categories
{
    public class CreateCategoryRequestDto
    {
        [Required(ErrorMessage = "שם הקטגוריה הוא שדה חובה.")]
        [StringLength(50, ErrorMessage = "שם הקטגוריה יכול להכיל עד 50 תווים.")]
        public string Name { get; set; } = string.Empty;
    }
}
