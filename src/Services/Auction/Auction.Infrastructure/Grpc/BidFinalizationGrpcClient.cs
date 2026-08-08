using Auctions.Application.Interfaces;
using BidService.API.Grpc;

namespace Auctions.Infrastructure.Grpc;

public sealed class BidFinalizationGrpcClient : IBidFinalizationClient
{
    private readonly BidGrpc.BidGrpcClient _client;

    public BidFinalizationGrpcClient(BidGrpc.BidGrpcClient client)
    {
        _client = client;
    }

    public async Task<WinningBidResult?> FinalizeAuctionAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.FinalizeAuctionAsync(
            new FinalizeAuctionRequest { AuctionId = auctionId.ToString() },
            cancellationToken: cancellationToken);

        if (!response.HasWinningBid || response.WinningBid is null)
            return null;

        if (!Guid.TryParse(response.WinningBid.BidderId, out var bidderId))
            throw new InvalidOperationException($"Bidding service returned an invalid bidder ID for auction {auctionId}.");

        return new WinningBidResult(
            bidderId,
            response.WinningBid.Bidder,
            response.WinningBid.AmountCents / 100m);
    }
}
