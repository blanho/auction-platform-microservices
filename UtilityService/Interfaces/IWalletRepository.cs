using UtilityService.Domain.Entities;

namespace UtilityService.Interfaces;

public interface IWalletRepository
{
    Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<WalletTransaction>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<(List<WalletTransaction> Items, int TotalCount)> GetPagedAsync(
        string username,
        TransactionType? type,
        TransactionStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(string username, CancellationToken cancellationToken = default);
    Task<decimal> GetPendingHoldsAsync(string username, CancellationToken cancellationToken = default);
    Task<decimal> GetPendingWithdrawalsAsync(string username, CancellationToken cancellationToken = default);
    Task<List<WalletTransaction>> GetPendingWithdrawalRequestsAsync(CancellationToken cancellationToken = default);
    Task<WalletTransaction> AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<WalletTransaction>> GetTimedOutPendingTransactionsAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(List<WalletTransaction> transactions, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<int> GetActiveUsersCountAsync(int daysActive = 30, CancellationToken cancellationToken = default);
}
