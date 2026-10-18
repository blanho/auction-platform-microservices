namespace Auctions.Application.Filtering;

public sealed class AuctionSortFields
{
    public decimal Price { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string Title { get; init; } = string.Empty;
}
