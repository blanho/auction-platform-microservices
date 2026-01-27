using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class AuctionView : BaseEntity
{
    public Guid AuctionId { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset ViewedAt { get; set; }
    
    public Auction? Auction { get; set; }
}
