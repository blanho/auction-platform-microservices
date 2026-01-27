using System.Text.Json.Serialization;

namespace Payment.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    PaymentPending,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Completed,
    Cancelled,
    Disputed,
    Refunded
}
