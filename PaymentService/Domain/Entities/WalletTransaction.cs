using Common.Domain.Entities;

namespace PaymentService.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ExternalTransactionId { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public void MarkAsProcessing()
    {
        EnsurePending();
        Status = TransactionStatus.Processing;
    }

    public void Complete(string? externalTransactionId = null)
    {
        if (Status != TransactionStatus.Pending && Status != TransactionStatus.Processing)
            throw new InvalidOperationException($"Cannot complete transaction in {Status} status");

        Status = TransactionStatus.Completed;
        ProcessedAt = DateTimeOffset.UtcNow;
        ExternalTransactionId = externalTransactionId;
    }

    public void Fail(string? reason = null)
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed transaction");

        Status = TransactionStatus.Failed;
        ProcessedAt = DateTimeOffset.UtcNow;
        if (reason != null)
            Description = $"{Description} - Failed: {reason}".Trim(' ', '-');
    }

    public void Cancel()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Can only cancel pending transactions");

        Status = TransactionStatus.Cancelled;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void SetExternalTransactionId(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("External transaction ID cannot be empty", nameof(externalId));

        ExternalTransactionId = externalId;
    }

    private void EnsurePending()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException($"Transaction is not in pending status. Current: {Status}");
    }
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
