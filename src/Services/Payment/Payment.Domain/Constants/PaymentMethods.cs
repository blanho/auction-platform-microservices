namespace Payment.Domain.Constants;

public static class PaymentMethods
{
    public const string Stripe = "stripe";
    public const string PayPal = "paypal";
    public const string Wallet = "wallet";
    public const string BankTransfer = "bank_transfer";
    public const string CreditCard = "credit_card";

    public static readonly string[] Allowed =
    [
        Stripe,
        PayPal,
        Wallet,
        BankTransfer,
        CreditCard
    ];

    public static bool IsValid(string method)
    {
        return Allowed.Contains(method, StringComparer.OrdinalIgnoreCase);
    }
}
