using BuildingBlocks.Domain.Entities;

namespace Bidding.Domain.Entities;

public class AutoBid : AggregateRoot
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
        var autoBid = new AutoBid
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

        autoBid.AddDomainEvent(new AutoBidCreatedDomainEvent
        {
            AutoBidId = autoBid.Id,
            AuctionId = auctionId,
            UserId = userId,
            Username = username,
            MaxAmount = maxAmount
        });

        return autoBid;
    }

    public void UpdateMaxAmount(decimal newMaxAmount)
    {
        MaxAmount = newMaxAmount;

        AddDomainEvent(new AutoBidMaxAmountUpdatedDomainEvent
        {
            AutoBidId = Id,
            AuctionId = AuctionId,
            UserId = UserId,
            NewMaxAmount = newMaxAmount
        });
    }

    public void RecordBid(decimal amount)
    {
        CurrentBidAmount = amount;
        LastBidAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;

        AddDomainEvent(new AutoBidActivatedDomainEvent
        {
            AutoBidId = Id,
            AuctionId = AuctionId,
            UserId = UserId
        });
    }

    public void Deactivate()
    {
        IsActive = false;

        AddDomainEvent(new AutoBidDeactivatedDomainEvent
        {
            AutoBidId = Id,
            AuctionId = AuctionId,
            UserId = UserId
        });
    }

    public bool CanBid(decimal currentHighBid)
    {
        return IsActive && MaxAmount > currentHighBid;
    }
}
