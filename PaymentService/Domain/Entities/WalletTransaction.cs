namespace PaymentService.Domain.Entities;

public class WalletTransaction
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; }
    public string PaymentMethod { get; set; }
    public string ExternalTransactionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Hold,
    Payment,
    Refund,
    Fee
}

public enum TransactionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
