using Payment.Domain.Entities;
using Payment.Domain.Constants;

namespace Payment.Application.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal HeldAmount { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Currency { get; set; } = WalletDefaults.DefaultCurrency;
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
    public string Description { get; set; } = null!;
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public string ExternalTransactionId { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}

public class CreateWalletTransactionDto
{
    public string Username { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
}
