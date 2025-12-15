using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

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

    public async Task<IEnumerable<WalletTransaction>> GetByUsernameAsync(string username, int page = 1, int pageSize = 10)
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
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<WalletTransaction> UpdateAsync(WalletTransaction transaction)
    {
        _context.WalletTransactions.Update(transaction);
        await _context.SaveChangesAsync();
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
