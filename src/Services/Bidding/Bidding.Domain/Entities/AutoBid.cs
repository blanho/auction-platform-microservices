using BuildingBlocks.Domain.Entities;

namespace Bidding.Domain.Entities;

public class AutoBid : BaseEntity
{
    public Guid AuctionId { get; private set; }
    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public decimal MaxAmount { get; private set; }
    public decimal CurrentBidAmount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? LastBidAt { get; private set; }

    public static AutoBid Create(Guid auctionId, Guid userId, string username, decimal maxAmount)
    {
        return new AutoBid
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            UserId = userId,
            Username = username,
            MaxAmount = maxAmount,
            CurrentBidAmount = 0,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateMaxAmount(decimal newMaxAmount)
    {
        MaxAmount = newMaxAmount;
    }

    public void RecordBid(decimal amount)
    {
        CurrentBidAmount = amount;
        LastBidAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool CanBid(decimal currentHighBid)
    {
        return IsActive && MaxAmount > currentHighBid;
    }
}
