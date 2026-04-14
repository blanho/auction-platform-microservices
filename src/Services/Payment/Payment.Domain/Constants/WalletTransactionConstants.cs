namespace Payment.Domain.Constants;

public static class WalletTransactionDescriptions
{
    public const string FundsReleased = "Funds released";
    public const string FundsHeld = "Funds held";
    public const string Deposit = "Deposit";
    public const string Withdrawal = "Withdrawal";
    public const string PaymentProcessed = "Payment processed";
    public const string RefundProcessed = "Refund processed";
}

public static class WalletReferenceTypes
{
    public const string Order = "Order";
    public const string Release = "Release";
    public const string Bid = "Bid";
    public const string Auction = "Auction";
    public const string Refund = "Refund";
}
