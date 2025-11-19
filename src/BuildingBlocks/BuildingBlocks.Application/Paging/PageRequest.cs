namespace BuildingBlocks.Application.Paging;

public abstract class PageRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public IReadOnlyList<SortDescriptor>? Sorts { get; init; }
}
