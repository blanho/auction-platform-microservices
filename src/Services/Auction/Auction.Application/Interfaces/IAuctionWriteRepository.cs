using Auctions.Domain.Entities;

namespace Auctions.Application.Interfaces;

public interface IAuctionWriteRepository
{
    Task<Auction?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Auction> CreateAsync(Auction auction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Auction auction, CancellationToken cancellationToken = default);
}
