using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Repository;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class WalletRepository : EfRepository<Wallet, PaymentDbContext>, IWalletRepository
{
    public WalletRepository(
        PaymentDbContext context, IDateTimeProvider clock, IAuditContext auditContext)
        : base(context, clock, auditContext)
    {
    }

    public async Task<Wallet?> GetByUsernameAsync(string username)
    {
        return await Context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Username == username);
    }

    Task<Wallet> IWalletRepository.AddAsync(Wallet wallet) => AddAsync(wallet);

    async Task<Wallet> IWalletRepository.UpdateAsync(Wallet wallet)
    {
        await base.UpdateAsync(wallet);
        return wallet;
    }

    protected override void ApplyCreatedAudit(Wallet entity, DateTimeOffset timestamp) =>
        entity.SetCreatedAudit(entity.CreatedBy, entity.CreatedAt);

    protected override Guid GetUpdatedBy(Wallet entity) => Guid.Empty;

    public async Task<bool> ExistsAsync(string username)
    {
        return await Context.Wallets
            .AsNoTracking()
            .AnyAsync(w => w.Username == username);
    }
}
