using BidService.Domain.Entities;

namespace BidService.Application.Interfaces;

public interface IAutoBidRepository
{
    Task<AutoBid?> GetByIdAsync(Guid id);
    Task<AutoBid?> GetActiveAutoBidAsync(Guid auctionId, Guid userId);
    Task<List<AutoBid>> GetActiveAutoBidsForAuctionAsync(Guid auctionId);
    Task<List<AutoBid>> GetAutoBidsByUserAsync(Guid userId);
    Task AddAsync(AutoBid autoBid);
    Task UpdateAsync(AutoBid autoBid);
    Task DeleteAsync(AutoBid autoBid);
}
