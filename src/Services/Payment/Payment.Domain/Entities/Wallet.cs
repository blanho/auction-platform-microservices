using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

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

    public void Deposit(decimal amount)
    {
        GuardActive();
        GuardPositiveAmount(amount);
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        GuardActive();
        GuardPositiveAmount(amount);

        if (AvailableBalance < amount)
        {
            throw new DomainInvariantException(
                $"Insufficient available balance. Available: {AvailableBalance}, Requested: {amount}");
        }

        Balance -= amount;
    }

    public void HoldFunds(decimal amount)
    {
        GuardActive();
        GuardPositiveAmount(amount);

        if (AvailableBalance < amount)
        {
            throw new DomainInvariantException(
                $"Insufficient available balance to hold. Available: {AvailableBalance}, Requested: {amount}");
        }

        HeldAmount += amount;
    }

    public void ReleaseFunds(decimal amount)
    {
        GuardActive();
        GuardPositiveAmount(amount);

        if (HeldAmount < amount)
        {
            throw new DomainInvariantException(
                $"Cannot release more than held amount. Held: {HeldAmount}, Requested: {amount}");
        }

        HeldAmount -= amount;
    }

    public void DeductFromHeld(decimal amount)
    {
        GuardActive();
        GuardPositiveAmount(amount);

        if (HeldAmount < amount)
        {
            throw new DomainInvariantException(
                $"Cannot deduct more than held amount. Held: {HeldAmount}, Requested: {amount}");
        }

        HeldAmount -= amount;
        Balance -= amount;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void GuardActive()
    {
        if (!IsActive)
        {
            throw new InvalidEntityStateException(
                nameof(Wallet), nameof(IsActive), "Wallet is deactivated");
        }
    }

    private static void GuardPositiveAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainInvariantException($"Amount must be positive. Received: {amount}");
        }
    }
}
