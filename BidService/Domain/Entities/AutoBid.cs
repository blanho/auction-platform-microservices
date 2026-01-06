using Common.Domain.Entities;

namespace BidService.Domain.Entities;

public class AutoBid : BaseEntity
{
    private decimal _maxAmount;
    private decimal _currentBidAmount;

    public Guid AuctionId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    public decimal MaxAmount
    {
        get => _maxAmount;
        set
        {
            ValidateMaxAmount(value);
            _maxAmount = value;
        }
    }

    public decimal CurrentBidAmount
    {
        get => _currentBidAmount;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Current bid amount cannot be negative");
            _currentBidAmount = value;
        }
    }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastBidAt { get; set; }
    public void UpdateMaxAmount(decimal newMaxAmount)
    {
        ValidateMaxAmount(newMaxAmount);

        if (newMaxAmount < CurrentBidAmount)
            throw new InvalidOperationException("New max amount cannot be less than current bid amount");

        _maxAmount = newMaxAmount;
    }

    public void RecordBid(decimal amount)
    {
        if (amount > MaxAmount)
            throw new InvalidOperationException("Bid amount exceeds max amount");

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
