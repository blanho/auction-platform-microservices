namespace Payment.Domain.Constants;

public static class OrderAuditActions
{
    public const string StatusUpdated = "StatusUpdated";
    public const string CheckoutPrepared = "CheckoutPrepared";
    public const string Delivered = "Delivered";
    public const string Shipped = "Shipped";
    public const string Cancelled = "Cancelled";
}
