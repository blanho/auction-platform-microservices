using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Domain.Entities;

namespace Payment.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;

    [Timestamp]
    public uint RowVersion { get; private set; }

    public decimal Balance { get; private set; }
    public decimal HeldAmount { get; private set; }
    public decimal AvailableBalance => Balance - HeldAmount;
    public string Currency { get; private set; } = "USD";
    public bool IsActive { get; private set; } = true;

    public static Wallet Create(Guid userId, string username, string currency = "USD")
    {
        return new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            Currency = currency,
            Balance = 0,
            HeldAmount = 0,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Deposit(decimal amount) => Balance += amount;

    public void Withdraw(decimal amount) => Balance -= amount;

    public void HoldFunds(decimal amount) => HeldAmount += amount;

    public void ReleaseFunds(decimal amount) => HeldAmount -= amount;

    public void DeductFromHeld(decimal amount)
    {
        HeldAmount -= amount;
        Balance -= amount;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
