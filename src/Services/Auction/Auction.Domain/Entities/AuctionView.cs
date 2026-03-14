using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class AuctionView : BaseEntity
{
    public Guid AuctionId { get; private set; }
    public string? UserId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTimeOffset ViewedAt { get; private set; }
    
    public Auction? Auction { get; private set; }

    private AuctionView() { }

    public static AuctionView Create(Guid auctionId, string? userId, string? ipAddress, DateTimeOffset viewedAt)
    {
        return new AuctionView
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            UserId = userId,
            IpAddress = ipAddress,
            ViewedAt = viewedAt
        };
    }
}
