using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace BuildingBlocks.Domain.ValueObjects;

public sealed class Money : ValueObject, IComparable<Money>
{
    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "SEK", "NOK"
    };

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainInvariantException("Money amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainInvariantException("Currency is required.");

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (!SupportedCurrencies.Contains(normalizedCurrency))
            throw new DomainInvariantException($"Currency '{currency}' is not supported.");

        Amount = Math.Round(amount, 2, MidpointRounding.ToEven);
        Currency = normalizedCurrency;
    }

    public static Money Zero(string currency = "USD") => new(0, currency);

    public static Money Of(decimal amount, string currency = "USD") => new(amount, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainInvariantException("Subtraction would result in negative money.");
        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new DomainInvariantException("Cannot multiply money by a negative factor.");
        return new Money(Amount * factor, Currency);
    }

    public Money Percentage(decimal percent)
    {
        if (percent is < 0 or > 100)
            throw new DomainInvariantException("Percentage must be between 0 and 100.");
        return new Money(Amount * percent / 100m, Currency);
    }

    public bool IsZero() => Amount == 0;

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsGreaterThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);
        return Amount >= other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount < other.Amount;
    }

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static bool operator >(Money left, Money right) => left.IsGreaterThan(right);
    public static bool operator <(Money left, Money right) => left.IsLessThan(right);
    public static bool operator >=(Money left, Money right) => left.IsGreaterThanOrEqual(right);
    public static bool operator <=(Money left, Money right) => !left.IsGreaterThan(right);

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
            throw new DomainInvariantException(
                $"Cannot operate on different currencies: {Currency} and {other.Currency}.");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} {Currency}";
}
