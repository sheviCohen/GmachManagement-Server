namespace Gmach.Core.Entities
{
    // קטגוריה/מטרה של הלוואה (חתונה, לימודים, חירום...).
    // קשר רבים-לרבים עם Loan: הלוואה יכולה להשתייך לכמה קטגוריות.
    public class LoanCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Loan> Loans { get; set; }
            = new List<Loan>();
    }
}
