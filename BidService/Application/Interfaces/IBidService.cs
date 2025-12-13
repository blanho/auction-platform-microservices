using BidService.Application.DTOs;

namespace BidService.Application.Interfaces
{
    public interface IBidService
    {
        Task<BidDto> PlaceBidAsync(PlaceBidDto dto, string bidder, CancellationToken cancellationToken = default);
        Task<List<BidDto>> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<List<BidDto>> GetBidsForBidderAsync(string bidder, CancellationToken cancellationToken = default);
    }

    public interface IAutoBidService
    {
        Task<AutoBidDto?> CreateAutoBidAsync(CreateAutoBidDto dto, string bidder, CancellationToken cancellationToken = default);
        Task<AutoBidDto?> GetAutoBidByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AutoBidDto?> GetActiveAutoBidAsync(Guid auctionId, string bidder, CancellationToken cancellationToken = default);
        Task<List<AutoBidDto>> GetAutoBidsByBidderAsync(string bidder, CancellationToken cancellationToken = default);
        Task<AutoBidDto?> UpdateAutoBidAsync(Guid id, UpdateAutoBidDto dto, string bidder, CancellationToken cancellationToken = default);
        Task<bool> CancelAutoBidAsync(Guid id, string bidder, CancellationToken cancellationToken = default);
        Task ProcessAutoBidsForAuctionAsync(Guid auctionId, int currentHighBid, CancellationToken cancellationToken = default);
    }
}
