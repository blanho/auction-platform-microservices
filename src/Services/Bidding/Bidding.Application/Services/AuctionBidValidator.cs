using Bidding.Application.Interfaces;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.Services;

public sealed class AuctionBidValidator(IAuctionSnapshotRepository snapshots, IAuctionGrpcClient auctions,
    IDateTimeProvider dateTime, ILogger<AuctionBidValidator> logger) : IAuctionBidValidator
{
    public async Task<AuctionValidationResult> ValidateAsync(PlaceBidDto request, Guid bidderId,
        string bidderUsername, CancellationToken cancellationToken = default)
    {
        var snapshot = await snapshots.GetAsync(request.AuctionId, cancellationToken);
        if (snapshot is null)
        {
            logger.LogInformation("Auction snapshot missing for {AuctionId}, falling back to gRPC validation", request.AuctionId);
            var result = await auctions.ValidateAuctionForBidAsync(request.AuctionId, bidderUsername, request.Amount, cancellationToken);
            if (!result.IsValid)
                logger.LogWarning("Auction validation failed for {AuctionId}: {ErrorCode} - {ErrorMessage}",
                    request.AuctionId, result.ErrorCode, result.ErrorMessage);
            return result;
        }

        if (!string.Equals(snapshot.Status, BidDefaults.AuctionStatuses.Live, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(snapshot.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Auction validation failed locally for {AuctionId}: Auction is not active. Current status: {Status}",
                request.AuctionId, snapshot.Status);
            return new(false, $"Auction is not active. Current status: {snapshot.Status}");
        }

        if (snapshot.EndTime < dateTime.UtcNow)
        {
            logger.LogWarning("Auction validation failed locally for {AuctionId}: Auction has ended", request.AuctionId);
            return new(false, "Auction has ended");
        }

        if (snapshot.SellerId == bidderId || string.Equals(snapshot.SellerUsername, bidderUsername, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Auction validation failed locally for {AuctionId}: Seller cannot bid", request.AuctionId);
            return new(false, "You cannot bid on your own auction");
        }

        return new(true, ReservePrice: snapshot.ReservePrice);
    }
}
