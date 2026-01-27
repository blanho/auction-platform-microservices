using BidService.Contracts.Grpc;
using Bidding.Application.Interfaces;
using Grpc.Core;

namespace Bidding.Api.Grpc;

public class BidStatsGrpcService(
    IBidRepository bidRepository,
    ILogger<BidStatsGrpcService> logger)
    : BidStatsGrpc.BidStatsGrpcBase
{
    private readonly IBidRepository _bidRepository = bidRepository;
    private readonly ILogger<BidStatsGrpcService> _logger = logger;

    public override async Task<BidStatsResponse> GetBidStats(
        GetBidStatsRequest request,
        ServerCallContext context)
    {
        var stats = await _bidRepository.GetBidStatsAsync(context.CancellationToken);

        return new BidStatsResponse
        {
            TotalBids = stats.TotalBids,
            UniqueBidders = stats.UniqueBidders,
            TotalBidAmount = decimal.ToDouble(stats.TotalBidAmount),
            AverageBidAmount = decimal.ToDouble(stats.AverageBidAmount),
            BidsToday = stats.BidsToday,
            BidsThisWeek = stats.BidsThisWeek,
            BidsThisMonth = stats.BidsThisMonth
        };
    }

    public override async Task<UserBidStatsResponse> GetUserBidStats(
        GetUserBidStatsRequest request,
        ServerCallContext context)
    {
        var userStats = await _bidRepository.GetUserBidStatsAsync(
            request.Username, 
            context.CancellationToken);

        return new UserBidStatsResponse
        {
            TotalBids = userStats.TotalBids,
            ActiveBids = userStats.ActiveBids,
            AuctionsWon = userStats.AuctionsWon,
            TotalAmountBid = decimal.ToDouble(userStats.TotalAmountBid),
            TotalAmountWon = decimal.ToDouble(userStats.TotalAmountWon)
        };
    }

    public override async Task<TopBiddersResponse> GetTopBidders(
        GetTopBiddersRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit > 0 ? request.Limit : 10;
        var topBidders = await _bidRepository.GetTopBiddersAsync(limit, context.CancellationToken);

        var response = new TopBiddersResponse();
        foreach (var bidder in topBidders)
        {
            response.Bidders.Add(new TopBidder
            {
                BidderId = bidder.BidderId.ToString(),
                Username = bidder.Username,
                BidCount = bidder.BidCount,
                TotalAmount = decimal.ToDouble(bidder.TotalAmount),
                AuctionsWon = bidder.AuctionsWon
            });
        }

        return response;
    }

    public override async Task<AuctionBidCountResponse> GetAuctionBidCount(
        GetAuctionBidCountRequest request,
        ServerCallContext context)
    {
        var auctionIds = request.AuctionIds
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        var bidCounts = await _bidRepository.GetBidCountsForAuctionsAsync(
            auctionIds, 
            context.CancellationToken);

        var response = new AuctionBidCountResponse();
        foreach (var kvp in bidCounts)
        {
            response.BidCounts.Add(kvp.Key.ToString(), kvp.Value);
        }

        return response;
    }
}
