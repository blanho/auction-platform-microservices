namespace Payment.Infrastructure.Constants;

public static class StripeEventTypes
{
    public const string PaymentIntentSucceeded = "payment_intent.succeeded";
    public const string PaymentIntentPaymentFailed = "payment_intent.payment_failed";
    public const string CheckoutSessionCompleted = "checkout.session.completed";
}

public static class StripePaymentModes
{
    public const string Payment = "payment";
}

public static class StripePaymentMethodTypes
{
    public const string Card = "card";
}

public static class StripeMetadataKeys
{
    public const string OrderId = "orderId";
    public const string UserId = "userId";
    public const string Username = "username";
}
