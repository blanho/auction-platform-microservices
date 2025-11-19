using BuildingBlocks.Domain.Exceptions;
using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    private TransactionStatus _status = TransactionStatus.Pending;
    
    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Balance { get; private set; }
    public TransactionStatus Status 
    { 
        get => _status; 
        private set => _status = value; 
    }
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
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
            
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
            _status = TransactionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    public void MarkAsProcessing()
    {
        EnsurePending();
        Status = TransactionStatus.Processing;
    }

    public void Complete(string? externalTransactionId = null)
    {
        if (Status != TransactionStatus.Pending && Status != TransactionStatus.Processing)
            throw new InvalidEntityStateException(nameof(WalletTransaction), Status.ToString(), "Can only complete Pending or Processing transactions");

        Status = TransactionStatus.Completed;
        ProcessedAt = DateTimeOffset.UtcNow;
        ExternalTransactionId = externalTransactionId;
    }

    public void Fail(string? reason = null)
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidEntityStateException(nameof(WalletTransaction), Status.ToString(), "Cannot fail a completed transaction");

        Status = TransactionStatus.Failed;
        ProcessedAt = DateTimeOffset.UtcNow;
        if (reason != null)
            Description = $"{Description} - Failed: {reason}".Trim(' ', '-');
    }

    public void Cancel()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidEntityStateException(nameof(WalletTransaction), Status.ToString(), "Can only cancel pending transactions");

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
            throw new InvalidEntityStateException(nameof(WalletTransaction), Status.ToString(), "Transaction must be in Pending status");
    }
}
