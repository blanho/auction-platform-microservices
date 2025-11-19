namespace Bidding.Application.Interfaces
{
    public interface IBidService
    {
        Task<BidDto> PlaceBidAsync(PlaceBidDto dto, Guid bidderId, string bidderUsername, CancellationToken cancellationToken = default);
        Task<BidDto> PlaceBidAsync(PlaceBidDto dto, Guid bidderId, string bidderUsername, bool isAutoBid, CancellationToken cancellationToken = default);
        Task<List<BidDto>> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<List<BidDto>> GetBidsForBidderAsync(string bidderUsername, CancellationToken cancellationToken = default);
    }

    public interface IAutoBidService
    {
        Task<AutoBidDto?> CreateAutoBidAsync(CreateAutoBidDto dto, Guid userId, string username, CancellationToken cancellationToken = default);
        Task<AutoBidDto?> GetAutoBidByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AutoBidDto?> GetActiveAutoBidAsync(Guid auctionId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<AutoBidDto>> GetAutoBidsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<AutoBidDto?> UpdateAutoBidAsync(Guid id, UpdateAutoBidDto dto, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CancelAutoBidAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task ProcessAutoBidsForAuctionAsync(Guid auctionId, decimal currentHighBid, CancellationToken cancellationToken = default);
    }
}

