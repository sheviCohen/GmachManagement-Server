using Gmach.Core.DTOs.Categories;

namespace Gmach.Core.DTOs.Loans
{
    public class LoanResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<CategoryDto> Categories { get; set; } = new();
    }
}
