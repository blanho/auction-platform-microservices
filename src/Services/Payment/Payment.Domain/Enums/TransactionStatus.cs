namespace Payment.Domain.Enums;

public enum TransactionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
