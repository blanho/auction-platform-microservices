using BuildingBlocks.Application.Constants;

namespace BuildingBlocks.Application.Paging;

public class QueryParameters
{
    private int _page = PaginationDefaults.DefaultPage;
    private int _pageSize = PaginationDefaults.DefaultPageSize;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? PaginationDefaults.DefaultPage : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1
            ? PaginationDefaults.DefaultPageSize
            : Math.Min(value, PaginationDefaults.MaxPageSize);
    }

    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
    public IReadOnlyList<SortDescriptor>? Sorts { get; init; }

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;

    public static QueryParameters Default => new();

    public static QueryParameters Create(int page, int pageSize, string? sortBy = null, bool sortDesc = true) =>
        new()
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDesc
        };
}

public class QueryParameters<TFilter> : QueryParameters where TFilter : class, new()
{
    public TFilter Filter { get; init; } = new();
}
