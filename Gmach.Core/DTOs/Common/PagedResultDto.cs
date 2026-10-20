namespace Gmach.Core.DTOs.Common
{
    // תוצאה מדופדפת: הפריטים של העמוד הנוכחי + מידע לבניית כפתורי דפדוף
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling(TotalCount / (double)PageSize)
            : 0;
    }
}
