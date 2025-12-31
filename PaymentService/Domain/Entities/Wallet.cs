using Common.Domain.Entities;

namespace PaymentService.Domain.Entities;

public class Wallet : BaseEntity
{
    private decimal _balance;
    private decimal _heldAmount;

    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    public decimal Balance
    {
        get => _balance;
        set => _balance = value;
    }

    public decimal HeldAmount
    {
        get => _heldAmount;
        set => _heldAmount = value;
    }

    public decimal AvailableBalance => Balance - HeldAmount;
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;

    // Domain behavior methods
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
            throw new InvalidOperationException("Insufficient available balance");

        _balance -= amount;
    }

    public void HoldFunds(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Hold amount must be positive");

        EnsureActive();

        if (amount > AvailableBalance)
            throw new InvalidOperationException("Insufficient available balance to hold");

        _heldAmount += amount;
    }

    public void ReleaseFunds(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Release amount must be positive");

        if (amount > HeldAmount)
            throw new InvalidOperationException("Cannot release more than held amount");

        _heldAmount -= amount;
    }

    public void DeductFromHeld(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Deduction amount must be positive");

        EnsureActive();

        if (amount > HeldAmount)
            throw new InvalidOperationException("Cannot deduct more than held amount");

        _heldAmount -= amount;
        _balance -= amount;
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        if (Balance > 0 || HeldAmount > 0)
            throw new InvalidOperationException("Cannot deactivate wallet with balance or held funds");

        IsActive = false;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("Wallet is not active");
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));
    }
}
