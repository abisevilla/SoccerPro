namespace SoccerPro.Application;

public class PageRequest
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public required string SortBy { get; set; }
    public required string SortDirection { get; set; }
    public int SafePageNumber => Math.Max(1, PageNumber);
    public int SafePageSize => Math.Clamp(PageSize, 1, 100);
}
