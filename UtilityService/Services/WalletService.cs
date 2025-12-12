using UtilityService.Domain.Entities;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletRepository walletRepository, ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _logger = logger;
    }

    public async Task<WalletBalanceDto> GetBalanceAsync(string username, CancellationToken cancellationToken = default)
    {
        var totalBalance = await _walletRepository.GetBalanceAsync(username, cancellationToken);
        var pendingHolds = await _walletRepository.GetPendingHoldsAsync(username, cancellationToken);
        var pendingWithdrawals = await _walletRepository.GetPendingWithdrawalsAsync(username, cancellationToken);

        return new WalletBalanceDto
        {
            TotalBalance = totalBalance,
            AvailableBalance = totalBalance - pendingHolds - pendingWithdrawals,
            PendingHolds = pendingHolds,
            PendingWithdrawals = pendingWithdrawals
        };
    }

    public async Task<PagedTransactionsDto> GetTransactionsAsync(
        string username,
        TransactionQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _walletRepository.GetPagedAsync(
            username,
            queryParams.Type,
            queryParams.Status,
            queryParams.PageNumber,
            queryParams.PageSize,
            cancellationToken);

        var transactions = items.Select(MapToDto).ToList();

        return new PagedTransactionsDto
        {
            Transactions = transactions,
            TotalCount = totalCount,
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<WalletTransactionDto> GetTransactionAsync(
        string username,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _walletRepository.GetByIdAsync(transactionId, cancellationToken);

        if (transaction == null || transaction.Username != username)
        {
            throw new KeyNotFoundException("Transaction not found");
        }

        return MapToDto(transaction);
    }

    public async Task<WalletTransactionDto> CreateDepositAsync(
        string username,
        CreateDepositDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than zero.");
        }

        var currentBalance = await _walletRepository.GetBalanceAsync(username, cancellationToken);

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            Username = username,
            Type = TransactionType.Deposit,
            Amount = dto.Amount,
            Balance = currentBalance + dto.Amount,
            Status = TransactionStatus.Completed,
            Description = "Wallet deposit",
            PaymentMethod = dto.PaymentMethodId,
            CreatedAt = DateTimeOffset.UtcNow,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        await _walletRepository.AddAsync(transaction, cancellationToken);

        _logger.LogInformation("Deposit of {Amount} completed for user {Username}", dto.Amount, username);

        return MapToDto(transaction);
    }

    public async Task<WalletTransactionDto> CreateWithdrawalAsync(
        string username,
        CreateWithdrawalDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be greater than zero.");
        }

        var balance = await GetBalanceAsync(username, cancellationToken);

        if (dto.Amount > balance.AvailableBalance)
        {
            throw new InvalidOperationException("Insufficient available balance.");
        }

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            Username = username,
            Type = TransactionType.Withdrawal,
            Amount = dto.Amount,
            Balance = balance.TotalBalance,
            Status = TransactionStatus.Pending,
            Description = "Withdrawal request",
            PaymentMethod = dto.PaymentMethodId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _walletRepository.AddAsync(transaction, cancellationToken);

        _logger.LogInformation("Withdrawal request of {Amount} created for user {Username}", dto.Amount, username);

        return MapToDto(transaction);
    }

    public async Task<List<AdminWithdrawalDto>> GetPendingWithdrawalsAsync(CancellationToken cancellationToken = default)
    {
        var withdrawals = await _walletRepository.GetPendingWithdrawalRequestsAsync(cancellationToken);

        return withdrawals.Select(w => new AdminWithdrawalDto
        {
            Id = w.Id,
            Username = w.Username,
            Amount = w.Amount,
            PaymentMethod = w.PaymentMethod,
            Status = w.Status.ToString(),
            RequestedAt = w.CreatedAt
        }).ToList();
    }

    public async Task ApproveWithdrawalAsync(
        Guid transactionId,
        string? externalTransactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _walletRepository.GetByIdAsync(transactionId, cancellationToken)
            ?? throw new KeyNotFoundException("Transaction not found");

        ValidateWithdrawalTransaction(transaction);

        var currentBalance = await _walletRepository.GetBalanceAsync(transaction.Username, cancellationToken);

        transaction.Status = TransactionStatus.Completed;
        transaction.Balance = currentBalance - transaction.Amount;
        transaction.ExternalTransactionId = externalTransactionId;
        transaction.ProcessedAt = DateTimeOffset.UtcNow;

        await _walletRepository.UpdateAsync(transaction, cancellationToken);

        _logger.LogInformation(
            "Withdrawal {TransactionId} approved for user {Username}, amount: {Amount}",
            transactionId, transaction.Username, transaction.Amount);
    }

    public async Task RejectWithdrawalAsync(
        Guid transactionId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _walletRepository.GetByIdAsync(transactionId, cancellationToken)
            ?? throw new KeyNotFoundException("Transaction not found");

        ValidateWithdrawalTransaction(transaction);

        transaction.Status = TransactionStatus.Failed;
        transaction.Description = $"Rejected: {reason}";
        transaction.ProcessedAt = DateTimeOffset.UtcNow;

        await _walletRepository.UpdateAsync(transaction, cancellationToken);

        _logger.LogInformation(
            "Withdrawal {TransactionId} rejected for user {Username}, reason: {Reason}",
            transactionId, transaction.Username, reason);
    }

    public async Task<PaymentStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var pendingWithdrawals = await _walletRepository.GetPendingWithdrawalRequestsAsync(cancellationToken);

        return new PaymentStatisticsDto
        {
            PendingWithdrawalsCount = pendingWithdrawals.Count,
            PendingWithdrawalsTotal = pendingWithdrawals.Sum(w => w.Amount)
        };
    }

    private static void ValidateWithdrawalTransaction(WalletTransaction transaction)
    {
        if (transaction.Type != TransactionType.Withdrawal)
        {
            throw new InvalidOperationException("Transaction is not a withdrawal request.");
        }

        if (transaction.Status != TransactionStatus.Pending)
        {
            throw new InvalidOperationException("Withdrawal is not in pending status.");
        }
    }

    private static WalletTransactionDto MapToDto(WalletTransaction transaction)
    {
        return new WalletTransactionDto
        {
            Id = transaction.Id,
            Type = transaction.Type.ToString(),
            Amount = transaction.Amount,
            Status = transaction.Status.ToString(),
            Description = transaction.Description,
            Reference = transaction.ReferenceId?.ToString(),
            CreatedAt = transaction.CreatedAt,
            ProcessedAt = transaction.ProcessedAt
        };
    }
}
