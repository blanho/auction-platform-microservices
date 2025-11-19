using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Payment.Domain.Entities;

public class Wallet : BaseEntity
{
    private decimal _balance;
    private decimal _heldAmount;
    private string _currency = "USD";
    private bool _isActive = true;

    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;

    [Timestamp]
    public uint RowVersion { get; private set; }

    public decimal Balance
    {
        get => _balance;
        private set
        {
            if (value < 0)
                throw new DomainInvariantException($"Wallet.Balance cannot be negative. Attempted: {value}");
            _balance = value;
        }
    }

    public decimal HeldAmount
    {
        get => _heldAmount;
        private set
        {
            if (value < 0)
                throw new DomainInvariantException($"Wallet.HeldAmount cannot be negative. Attempted: {value}");
            _heldAmount = value;
        }
    }

    public static Wallet Create(Guid userId, string username, string currency = "USD")
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
        ValidateCurrency(currency);

        return new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            _currency = currency,
            _balance = 0,
            _heldAmount = 0,
            _isActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public decimal AvailableBalance
    {
        get
        {
            var available = Balance - HeldAmount;
            if (available < 0)
            {
                throw new DomainInvariantException(
                    $"Wallet.AvailableBalance is negative (Balance: {Balance}, HeldAmount: {HeldAmount}). Data integrity issue.");
            }
            return available;
        }
    }
    public string Currency
    {
        get => _currency;
        private set
        {
            ValidateCurrency(value);
            _currency = value;
        }
    }

    public bool IsActive
    {
        get => _isActive;
        private set => _isActive = value;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be positive");

        EnsureActive();
        _balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Withdrawal amount must be positive");

        EnsureActive();

        if (amount > AvailableBalance)
            throw new InvalidEntityStateException(nameof(Wallet), "Active", "Insufficient available balance for withdrawal");

        _balance -= amount;
    }

    public void HoldFunds(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Hold amount must be positive");

        EnsureActive();

        if (amount > AvailableBalance)
            throw new InvalidEntityStateException(nameof(Wallet), "Active", "Insufficient available balance to hold funds");

        _heldAmount += amount;
    }

    public void ReleaseFunds(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Release amount must be positive");

        if (amount > HeldAmount)
            throw new InvalidEntityStateException(nameof(Wallet), HeldAmount.ToString(), "Cannot release more than held amount");

        _heldAmount -= amount;
    }

    public void DeductFromHeld(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Deduction amount must be positive");

        EnsureActive();

        if (amount > HeldAmount)
            throw new InvalidEntityStateException(nameof(Wallet), HeldAmount.ToString(), "Cannot deduct more than held amount");

        _heldAmount -= amount;
        _balance -= amount;
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        if (Balance > 0 || HeldAmount > 0)
            throw new InvalidEntityStateException(nameof(Wallet), "Active", "Cannot deactivate wallet with balance or held funds");

        IsActive = false;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidEntityStateException(nameof(Wallet), "Inactive", "Wallet must be active for this operation");
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));
    }
}
