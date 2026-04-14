using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Bidding.Domain.ValueObjects;

public sealed class BidAmount : ValueObject, IComparable<BidAmount>
{
    public decimal Value { get; }
    public string Currency { get; }

    private BidAmount(decimal value, string currency)
    {
        Value = value;
        Currency = currency;
    }

    public static BidAmount Create(decimal amount, string currency = "USD")
    {
        if (amount <= 0)
            throw new DomainInvariantException("Bid amount must be positive.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainInvariantException("Currency is required.");

        return new BidAmount(Math.Round(amount, 2, MidpointRounding.ToEven), currency.ToUpperInvariant());
    }

    public bool MeetsMinimumIncrement(BidAmount currentHigh, decimal minimumIncrement) =>
        Value >= currentHigh.Value + minimumIncrement;

    public bool MeetsReservePrice(decimal reservePrice) =>
        Value >= reservePrice;

    public bool ExceedsBuyNowPrice(decimal buyNowPrice) =>
        Value >= buyNowPrice;

    public int CompareTo(BidAmount? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public static bool operator >(BidAmount left, BidAmount right) =>
        left.Value > right.Value;

    public static bool operator <(BidAmount left, BidAmount right) =>
        left.Value < right.Value;

    public static bool operator >=(BidAmount left, BidAmount right) =>
        left.Value >= right.Value;

    public static bool operator <=(BidAmount left, BidAmount right) =>
        left.Value <= right.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Currency;
    }

    public override string ToString() =>
        $"{Value:F2} {Currency}";
}
