using BuildingBlocks.Application.Constants;

namespace BuildingBlocks.Application.Abstractions;

public record PaginatedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PaginatedResult<T> Empty(int page = 1, int pageSize = 20) =>
        new(Array.Empty<T>(), 0, page, pageSize);
}
