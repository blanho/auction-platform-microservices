namespace Payment.Domain.Enums;

public enum OrderStatus
{
    PendingPayment,
    PaymentReceived,
    Processing,
    Shipped,
    Delivered,
    Completed,
    Cancelled,
    Disputed,
    Refunded
}
