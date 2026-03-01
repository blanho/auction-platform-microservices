using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Payment.Domain.Entities;

public class Wallet : AggregateRoot
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
        var wallet = new Wallet
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

        wallet.AddDomainEvent(new WalletCreatedDomainEvent
        {
            WalletId = wallet.Id,
            UserId = userId,
            Username = username,
            Currency = currency
        });

        return wallet;
    }

    public void Deposit(decimal amount)
    {
        GuardActive();
        GuardPositiveAmount(amount);
        Balance += amount;

        AddDomainEvent(new FundsDepositedDomainEvent
        {
            WalletId = Id,
            UserId = UserId,
            Amount = amount,
            NewBalance = Balance
        });
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

        AddDomainEvent(new FundsWithdrawnDomainEvent
        {
            WalletId = Id,
            UserId = UserId,
            Amount = amount,
            NewBalance = Balance
        });
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

        AddDomainEvent(new FundsHeldDomainEvent
        {
            WalletId = Id,
            UserId = UserId,
            Amount = amount,
            NewHeldAmount = HeldAmount
        });
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

        AddDomainEvent(new FundsReleasedDomainEvent
        {
            WalletId = Id,
            UserId = UserId,
            Amount = amount,
            NewHeldAmount = HeldAmount
        });
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

        AddDomainEvent(new FundsDeductedFromHeldDomainEvent
        {
            WalletId = Id,
            UserId = UserId,
            Amount = amount,
            NewBalance = Balance,
            NewHeldAmount = HeldAmount
        });
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
