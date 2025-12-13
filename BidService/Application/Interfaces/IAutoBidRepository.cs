using BidService.Domain.Entities;

namespace BidService.Application.Interfaces;

public interface IAutoBidRepository
{
    Task<AutoBid?> GetByIdAsync(Guid id);
    Task<AutoBid?> GetActiveAutoBidAsync(Guid auctionId, string bidder);
    Task<List<AutoBid>> GetActiveAutoBidsForAuctionAsync(Guid auctionId);
    Task<List<AutoBid>> GetAutoBidsByBidderAsync(string bidder);
    Task AddAsync(AutoBid autoBid);
    Task UpdateAsync(AutoBid autoBid);
    Task DeleteAsync(AutoBid autoBid);
}
