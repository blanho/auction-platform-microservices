using BidService.API.Grpc;
using BidService.Application.Interfaces;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace BidService.API.Services
{
    [Authorize]
    public class BidGrpcService : BidGrpc.BidGrpcBase
    {
        private readonly IBidRepository _bidRepository;
        private readonly ILogger<BidGrpcService> _logger;

        public BidGrpcService(IBidRepository bidRepository, ILogger<BidGrpcService> logger)
        {
            _bidRepository = bidRepository;
            _logger = logger;
        }

        public override async Task<BidResponse> GetHighestBid(GetHighestBidRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetHighestBid called for auction {AuctionId}", request.AuctionId);

            if (!Guid.TryParse(request.AuctionId, out var auctionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid auction ID format"));
            }

            var bid = await _bidRepository.GetHighestBidForAuctionAsync(auctionId, context.CancellationToken);

            if (bid == null)
            {
                return new BidResponse();
            }

            return new BidResponse
            {
                Id = bid.Id.ToString(),
                AuctionId = bid.AuctionId.ToString(),
                Bidder = bid.BidderUsername,
                Amount = (double)bid.Amount,
                BidTime = bid.BidTime.ToString("O"),
                Status = bid.Status.ToString()
            };
        }

        public override async Task<BidListResponse> GetBidsForAuction(GetBidsForAuctionRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetBidsForAuction called for auction {AuctionId}", request.AuctionId);

            if (!Guid.TryParse(request.AuctionId, out var auctionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid auction ID format"));
            }

            var bids = await _bidRepository.GetBidsByAuctionIdAsync(auctionId, context.CancellationToken);

            var response = new BidListResponse
            {
                TotalCount = bids.Count
            };

            foreach (var bid in bids)
            {
                response.Bids.Add(new BidResponse
                {
                    Id = bid.Id.ToString(),
                    AuctionId = bid.AuctionId.ToString(),
                    Bidder = bid.BidderUsername,
                    Amount = (double)bid.Amount,
                    BidTime = bid.BidTime.ToString("O"),
                    Status = bid.Status.ToString()
                });
            }

            return response;
        }

        public override async Task<HasUserBidResponse> HasUserBidOnAuction(HasUserBidRequest request, ServerCallContext context)
        {
            _logger.LogInformation("HasUserBidOnAuction called for user {Bidder} on auction {AuctionId}", 
                request.Bidder, request.AuctionId);

            if (!Guid.TryParse(request.AuctionId, out var auctionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid auction ID format"));
            }

            var bids = await _bidRepository.GetBidsByAuctionIdAsync(auctionId, context.CancellationToken);
            var userBid = bids.FirstOrDefault(b => b.BidderUsername.Equals(request.Bidder, StringComparison.OrdinalIgnoreCase));

            var response = new HasUserBidResponse
            {
                HasBid = userBid != null
            };

            if (userBid != null)
            {
                response.HighestBid = new BidResponse
                {
                    Id = userBid.Id.ToString(),
                    AuctionId = userBid.AuctionId.ToString(),
                    Bidder = userBid.BidderUsername,
                    Amount = (double)userBid.Amount,
                    BidTime = userBid.BidTime.ToString("O"),
                    Status = userBid.Status.ToString()
                };
            }

            return response;
        }

        public override async Task<BidStatsResponse> GetBidStats(GetBidStatsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetBidStats called");

            var stats = await _bidRepository.GetBidStatsAsync(context.CancellationToken);
            var dailyStats = await _bidRepository.GetDailyBidStatsAsync(30, context.CancellationToken);

            var response = new BidStatsResponse
            {
                TotalBids = stats.TotalBids,
                UniqueBidders = stats.UniqueBidders,
                TotalBidAmount = (double)stats.TotalBidAmount,
                AverageBidAmount = (double)stats.AverageBidAmount,
                BidsToday = stats.BidsToday,
                BidsThisWeek = stats.BidsThisWeek,
                BidsThisMonth = stats.BidsThisMonth
            };

            foreach (var daily in dailyStats)
            {
                response.DailyStats.Add(new DailyBidStat
                {
                    Date = daily.Date.ToString("yyyy-MM-dd"),
                    BidCount = daily.BidCount,
                    TotalAmount = (double)daily.TotalAmount
                });
            }

            return response;
        }

        public override async Task<TopBiddersResponse> GetTopBidders(GetTopBiddersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetTopBidders called with limit {Limit}", request.Limit);

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
                    TotalAmount = (double)bidder.TotalAmount,
                    AuctionsWon = bidder.AuctionsWon
                });
            }

            return response;
        }
    }
}
