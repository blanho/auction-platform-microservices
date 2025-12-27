using BidService.Application.DTOs;

namespace BidService.Application.Interfaces;

public interface IAuctionValidationService
{
    Task<AuctionValidationResult> ValidateAuctionForBidAsync(
        Guid auctionId,
        string bidder,
        decimal bidAmount,
        CancellationToken cancellationToken = default);

    Task<AuctionInfo?> GetAuctionDetailsAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);

    Task<bool> ExtendAuctionAsync(
        Guid auctionId,
        int extendMinutes,
        string reason,
        CancellationToken cancellationToken = default);
}
