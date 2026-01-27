#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class Review : BaseEntity
{
    public Guid AuctionId { get; set; }
    public Auction? Auction { get; set; }
    public Guid? OrderId { get; set; }

    public Guid ReviewerId { get; set; }
    public string ReviewerUsername { get; set; } = string.Empty;

    public Guid ReviewedUserId { get; set; }
    public string ReviewedUsername { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? SellerResponse { get; set; }
    public DateTimeOffset? SellerResponseAt { get; set; }
    public void AddSellerResponse(string response)
    {
        SellerResponse = response;
        SellerResponseAt = DateTimeOffset.UtcNow;
    }

    public void RemoveSellerResponse()
    {
        SellerResponse = null;
        SellerResponseAt = null;
    }
}
