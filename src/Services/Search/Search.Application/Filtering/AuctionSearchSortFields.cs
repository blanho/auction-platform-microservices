namespace Search.Application.Filtering;

public sealed class AuctionSearchSortFields
{
    public decimal Price { get; init; }
    public DateTimeOffset? EndTime { get; init; }
    public int Bids { get; init; }
    public DateTimeOffset Created { get; init; }
}
