using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Payment.Domain.ValueObjects;

public sealed class PaymentAmount : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private PaymentAmount(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static PaymentAmount Create(decimal amount, string currency)
    {
        if (amount <= 0)
            throw new DomainInvariantException("Payment amount must be positive.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainInvariantException("Currency is required for payment.");

        return new PaymentAmount(
            Math.Round(amount, 2, MidpointRounding.ToEven),
            currency.Trim().ToUpperInvariant());
    }

    public static PaymentAmount Zero(string currency = "USD") => new(0, currency);

    public PaymentAmount ApplyPlatformFee(decimal feePercentage)
    {
        if (feePercentage is < 0 or > 100)
            throw new DomainInvariantException("Fee percentage must be between 0 and 100.");

        var fee = Amount * feePercentage / 100m;
        return new PaymentAmount(Math.Round(fee, 2, MidpointRounding.ToEven), Currency);
    }

    public PaymentAmount CalculateSellerPayout(decimal platformFeePercentage)
    {
        var fee = ApplyPlatformFee(platformFeePercentage);
        return new PaymentAmount(Amount - fee.Amount, Currency);
    }

    public bool IsRefundable(PaymentAmount maxRefund) =>
        Amount <= maxRefund.Amount;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() =>
        $"{Amount:F2} {Currency}";
}
