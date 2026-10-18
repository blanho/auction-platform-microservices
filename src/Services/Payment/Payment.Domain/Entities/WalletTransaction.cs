using BuildingBlocks.Domain.Guards;
using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    private WalletTransaction() { }

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
        decimal balanceAfter) => Create(new CreateWalletTransactionParams
        {
            UserId = userId, Username = username, Type = type, Amount = amount, BalanceAfter = balanceAfter
        });

    public static WalletTransaction Create(CreateWalletTransactionParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        Guard.AgainstEmpty(parameters.UserId, nameof(parameters.UserId));
        Guard.AgainstNullOrEmpty(parameters.Username, nameof(parameters.Username));

        var transaction = new WalletTransaction
        {
            UserId = parameters.UserId,
            Username = parameters.Username,
            Type = parameters.Type,
            Amount = parameters.Amount,
            Balance = parameters.BalanceAfter,
            Description = parameters.Description,
            ReferenceId = parameters.ReferenceId,
            ReferenceType = parameters.ReferenceType,
            PaymentMethod = parameters.PaymentMethod,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        return transaction;
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
