using System.Text.Json.Serialization;

namespace Payment.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    Deposit,
    Withdrawal,
    Hold,
    Release,
    Payment,
    Refund,
    Fee
}
