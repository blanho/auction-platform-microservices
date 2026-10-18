namespace Auctions.Domain.Entities;

public sealed record CreateReviewParams
{
    public required Guid AuctionId { get; init; }
    public Auction? Auction { get; init; }
    public Guid? OrderId { get; init; }
    public required Guid ReviewerId { get; init; }
    public required string ReviewerUsername { get; init; }
    public required Guid ReviewedUserId { get; init; }
    public required string ReviewedUsername { get; init; }
    public required int Rating { get; init; }
    public string? Title { get; init; }
    public string? Comment { get; init; }
}
