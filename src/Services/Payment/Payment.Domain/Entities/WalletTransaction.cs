using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Balance { get; private set; }
    public TransactionStatus Status { get; private set; } = TransactionStatus.Pending;
    public string? Description { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public static WalletTransaction Create(
        Guid userId,
        string username,
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        string? description = null,
        Guid? referenceId = null,
        string? referenceType = null,
        string? paymentMethod = null)
    {
        return new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            Type = type,
            Amount = amount,
            Balance = balanceAfter,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            PaymentMethod = paymentMethod,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsProcessing() => Status = TransactionStatus.Processing;

    public void Complete(string? externalTransactionId = null)
    {
        Status = TransactionStatus.Completed;
        ProcessedAt = DateTimeOffset.UtcNow;
        ExternalTransactionId = externalTransactionId;
    }

    public void Fail(string? reason = null)
    {
        Status = TransactionStatus.Failed;
        ProcessedAt = DateTimeOffset.UtcNow;
        if (reason != null)
            Description = $"{Description} - Failed: {reason}".Trim(' ', '-');
    }

    public void Cancel()
    {
        Status = TransactionStatus.Cancelled;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void SetExternalTransactionId(string externalId) => ExternalTransactionId = externalId;
}
