using Microsoft.EntityFrameworkCore;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly PaymentDbContext _context;

    public WalletRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet> GetByUsernameAsync(string username)
    {
        return await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Username == username);
    }

    public async Task<Wallet> AddAsync(Wallet wallet)
    {
        await _context.Wallets.AddAsync(wallet);
        return wallet;
    }

    public async Task<Wallet> UpdateAsync(Wallet wallet)
    {
        wallet.SetUpdatedAudit(Guid.Empty, DateTimeOffset.UtcNow);
        _context.Wallets.Update(wallet);
        return wallet;
    }

    public async Task<bool> ExistsAsync(string username)
    {
        return await _context.Wallets
            .AsNoTracking()
            .AnyAsync(w => w.Username == username);
    }
}
