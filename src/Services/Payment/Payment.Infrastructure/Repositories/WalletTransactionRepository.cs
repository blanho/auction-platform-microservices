
using Microsoft.EntityFrameworkCore;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly PaymentDbContext _context;

    public WalletTransactionRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<WalletTransaction> GetByIdAsync(Guid id)
    {
        return await _context.WalletTransactions.FindAsync(id);
    }

    public async Task<WalletTransaction?> GetByReferenceIdAsync(string referenceId, TransactionType type, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(referenceId, out var refGuid))
            return null;
            
        return await _context.WalletTransactions
            .Where(t => t.ReferenceId == refGuid && t.Type == type)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByUsernameAsync(string username, int page = PaginationDefaults.DefaultPage, int pageSize = PaginationDefaults.DefaultPageSize)
    {
        return await _context.WalletTransactions
            .Where(t => t.Username == username)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<WalletTransaction> AddAsync(WalletTransaction transaction)
    {
        await _context.WalletTransactions.AddAsync(transaction);
        return transaction;
    }

    public async Task<WalletTransaction> UpdateAsync(WalletTransaction transaction)
    {
        _context.WalletTransactions.Update(transaction);
        return transaction;
    }

    public async Task<int> GetCountByUsernameAsync(string username)
    {
        return await _context.WalletTransactions.CountAsync(t => t.Username == username);
    }

    public async Task<decimal> GetTotalByUsernameAndTypeAsync(string username, TransactionType type)
    {
        return await _context.WalletTransactions
            .Where(t => t.Username == username && t.Type == type && t.Status == TransactionStatus.Completed)
            .SumAsync(t => t.Amount);
    }
}
