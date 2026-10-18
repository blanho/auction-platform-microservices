using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

public sealed record CreateWalletTransactionParams
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public required TransactionType Type { get; init; }
    public required decimal Amount { get; init; }
    public required decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public string? PaymentMethod { get; init; }
}
