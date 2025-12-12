using UtilityService.Domain.Entities;
using UtilityService.DTOs;

namespace UtilityService.Interfaces;

public interface IWalletService
{
    Task<WalletBalanceDto> GetBalanceAsync(string username, CancellationToken cancellationToken = default);
    Task<PagedTransactionsDto> GetTransactionsAsync(string username, TransactionQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> GetTransactionAsync(string username, Guid transactionId, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> CreateDepositAsync(string username, CreateDepositDto dto, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> CreateWithdrawalAsync(string username, CreateWithdrawalDto dto, CancellationToken cancellationToken = default);
    Task<List<AdminWithdrawalDto>> GetPendingWithdrawalsAsync(CancellationToken cancellationToken = default);
    Task ApproveWithdrawalAsync(Guid transactionId, string? externalTransactionId, CancellationToken cancellationToken = default);
    Task RejectWithdrawalAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);
    Task<PaymentStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
