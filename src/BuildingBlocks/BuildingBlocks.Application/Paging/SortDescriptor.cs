namespace BuildingBlocks.Application.Paging;

public sealed class SortDescriptor
{
    public required string Field { get; init; }
    public bool Desc { get; init; }
}
