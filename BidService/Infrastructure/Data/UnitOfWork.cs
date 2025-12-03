using BidService.Application.Interfaces;
using BidService.Infrastructure.Data;

namespace BidService.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BidDbContext _context;

        public UnitOfWork(BidDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
