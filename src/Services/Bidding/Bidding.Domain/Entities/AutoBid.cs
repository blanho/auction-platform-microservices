using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Bidding.Domain.Entities;

public class AutoBid : BaseEntity
{
    private decimal _maxAmount;
    private decimal _currentBidAmount;
    private bool _isActive = true;

    public Guid AuctionId { get; private set; }
    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;

    public decimal MaxAmount
    {
        get => _maxAmount;
        private set
        {
            ValidateMaxAmount(value);
            _maxAmount = value;
        }
    }

    public decimal CurrentBidAmount
    {
        get => _currentBidAmount;
        private set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Current bid amount cannot be negative");
            _currentBidAmount = value;
        }
    }

    public bool IsActive
    {
        get => _isActive;
        private set => _isActive = value;
    }

    public DateTimeOffset? LastBidAt { get; private set; }

    public static AutoBid Create(Guid auctionId, Guid userId, string username, decimal maxAmount)
    {
        ValidateMaxAmount(maxAmount);
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
        ValidateMaxAmount(newMaxAmount);

        if (newMaxAmount < CurrentBidAmount)
            throw new DomainInvariantException("New max amount cannot be less than current bid amount");

        _maxAmount = newMaxAmount;
    }

    public void RecordBid(decimal amount)
    {
        if (amount > MaxAmount)
            throw new DomainInvariantException("Bid amount exceeds max amount");

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

    private static void ValidateMaxAmount(decimal maxAmount)
    {
        if (maxAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAmount), "Max amount must be greater than zero");

        if (maxAmount > 10_000_000)
            throw new ArgumentOutOfRangeException(nameof(maxAmount), "Max amount cannot exceed 10 million");
    }
}
