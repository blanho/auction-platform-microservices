using Payment.Domain.Entities;

namespace Payment.Application.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal HeldAmount { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class WalletTransactionDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public TransactionStatus Status { get; set; }
    public string Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; }
    public string PaymentMethod { get; set; }
    public string ExternalTransactionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}

public class CreateWalletTransactionDto
{
    public string Username { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; }
    public string PaymentMethod { get; set; }
}

public class DepositDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string Description { get; set; }
}

public class WithdrawDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string Description { get; set; }
}
