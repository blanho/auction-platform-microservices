using BidService.Application.DTOs;

namespace BidService.Application.Interfaces
{
    public interface IBidService
    {
        Task<BidDto> PlaceBidAsync(PlaceBidDto dto, string bidder, CancellationToken cancellationToken = default);
        Task<List<BidDto>> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<List<BidDto>> GetBidsForBidderAsync(string bidder, CancellationToken cancellationToken = default);
    }
}
