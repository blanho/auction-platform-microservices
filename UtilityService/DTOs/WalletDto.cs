using UtilityService.Domain.Entities;

namespace UtilityService.DTOs;

public class WalletTransactionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}

public class WalletBalanceDto
{
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PendingHolds { get; set; }
    public decimal PendingWithdrawals { get; set; }
}

public class CreateDepositDto
{
    public decimal Amount { get; set; }
    public string? PaymentMethodId { get; set; }
}

public class CreateWithdrawalDto
{
    public decimal Amount { get; set; }
    public string? PaymentMethodId { get; set; }
}

public class TransactionQueryParams
{
    public TransactionType? Type { get; set; }
    public TransactionStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedTransactionsDto
{
    public List<WalletTransactionDto> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class AdminWithdrawalDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
}

public class ProcessWithdrawalDto
{
    public string? ExternalTransactionId { get; set; }
}

public class RejectWithdrawalDto
{
    public string Reason { get; set; } = string.Empty;
}

public class PaymentStatisticsDto
{
    public int PendingWithdrawalsCount { get; set; }
    public decimal PendingWithdrawalsTotal { get; set; }
}
