using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.Domain.Entities;
using UtilityService.Interfaces;

namespace UtilityService.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly UtilityDbContext _context;

    public WalletRepository(UtilityDbContext context)
    {
        _context = context;
    }

    public async Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<WalletTransaction>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.Username == username)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<WalletTransaction> Items, int TotalCount)> GetPagedAsync(
        string username,
        TransactionType? type,
        TransactionStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WalletTransactions
            .Where(t => t.Username == username);

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<decimal> GetBalanceAsync(string username, CancellationToken cancellationToken = default)
    {
        var lastTransaction = await _context.WalletTransactions
            .Where(t => t.Username == username && t.Status == TransactionStatus.Completed)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return lastTransaction?.Balance ?? 0m;
    }

    public async Task<decimal> GetPendingHoldsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.Username == username && 
                       t.Type == TransactionType.Hold && 
                       t.Status == TransactionStatus.Pending)
            .SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<decimal> GetPendingWithdrawalsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.Username == username && 
                       t.Type == TransactionType.Withdrawal && 
                       t.Status == TransactionStatus.Pending)
            .SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<List<WalletTransaction>> GetPendingWithdrawalRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.Type == TransactionType.Withdrawal && 
                       t.Status == TransactionStatus.Pending)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WalletTransaction> AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.WalletTransactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<WalletTransaction>> GetTimedOutPendingTransactionsAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTimeOffset.UtcNow - timeout;
        
        return await _context.WalletTransactions
            .Where(t => t.Status == TransactionStatus.Pending &&
                       t.CreatedAt <= cutoffTime)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(List<WalletTransaction> transactions, CancellationToken cancellationToken = default)
    {
        _context.WalletTransactions.UpdateRange(transactions);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.WalletTransactions
            .Where(t => t.Type == TransactionType.Fee && t.Status == TransactionStatus.Completed);

        if (startDate.HasValue)
        {
            var start = new DateTimeOffset(startDate.Value, TimeSpan.Zero);
            query = query.Where(t => t.CreatedAt >= start);
        }

        if (endDate.HasValue)
        {
            var end = new DateTimeOffset(endDate.Value, TimeSpan.Zero);
            query = query.Where(t => t.CreatedAt <= end);
        }

        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<int> GetActiveUsersCountAsync(int daysActive = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysActive);
        
        return await _context.WalletTransactions
            .Where(t => t.CreatedAt >= cutoffDate)
            .Select(t => t.Username)
            .Distinct()
            .CountAsync(cancellationToken);
    }
}
