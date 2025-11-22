using AuctionService.Application.Interfaces;
using AuctionService.Infrastructure.Data;

namespace AuctionService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuctionDbContext _context;

    public UnitOfWork(AuctionDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
