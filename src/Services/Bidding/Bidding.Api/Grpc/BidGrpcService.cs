using BidService.API.Grpc;
using Bidding.Application.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Bidding.Api.Grpc;

public class BidGrpcService : BidGrpc.BidGrpcBase
{
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<BidGrpcService> _logger;

    public BidGrpcService(IBidRepository bidRepository, ILogger<BidGrpcService> logger)
    {
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public override async Task<BidResponse> GetHighestBid(
        GetHighestBidRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetHighestBid for auction {AuctionId}", request.AuctionId);

        if (!Guid.TryParse(request.AuctionId, out var auctionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid auction ID format"));

        var bid = await _bidRepository.GetHighestBidForAuctionAsync(auctionId, context.CancellationToken);

        if (bid == null)
            throw new RpcException(new Status(StatusCode.NotFound, "No bids found for this auction"));

        return MapToResponse(bid);
    }

    public override async Task<BidListResponse> GetBidsForAuction(
        GetBidsForAuctionRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("GetBidsForAuction for auction {AuctionId}", request.AuctionId);

        if (!Guid.TryParse(request.AuctionId, out var auctionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid auction ID format"));

        var bids = await _bidRepository.GetBidsByAuctionIdAsync(auctionId, context.CancellationToken);

        var response = new BidListResponse { TotalCount = bids.Count };
        response.Bids.AddRange(bids.Select(MapToResponse));
        return response;
    }

    public override async Task<HasUserBidResponse> HasUserBidOnAuction(
        HasUserBidRequest request,
        ServerCallContext context)
    {
        _logger.LogDebug("HasUserBidOnAuction for auction {AuctionId}, bidder {Bidder}",
            request.AuctionId, request.Bidder);

        if (!Guid.TryParse(request.AuctionId, out var auctionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid auction ID format"));

        var bids       = await _bidRepository.GetBidsByAuctionIdAsync(auctionId, context.CancellationToken);
        var userBids   = bids.Where(b => b.BidderUsername.Equals(request.Bidder, StringComparison.OrdinalIgnoreCase)).ToList();
        var hasBid     = userBids.Count > 0;
        var highestBid = userBids.OrderByDescending(b => b.Amount).FirstOrDefault();

        var response = new HasUserBidResponse { HasBid = hasBid };
        if (highestBid != null)
            response.HighestBid = MapToResponse(highestBid);

        return response;
    }

    private static BidResponse MapToResponse(Bidding.Domain.Entities.Bid bid) => new()
    {
        Id        = bid.Id.ToString(),
        AuctionId = bid.AuctionId.ToString(),
        Bidder    = bid.BidderUsername,
        AmountCents = DecimalToCents(bid.Amount),
        BidTime   = bid.BidTime.ToString("O"),
        Status    = bid.Status.ToString()
    };

    private static long DecimalToCents(decimal amount) =>
        (long)decimal.Round(amount * 100, MidpointRounding.AwayFromZero);
}

