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
                Bidder = bid.Bidder,
                Amount = bid.Amount,
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
                    Bidder = bid.Bidder,
                    Amount = bid.Amount,
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
            var userBid = bids.FirstOrDefault(b => b.Bidder.Equals(request.Bidder, StringComparison.OrdinalIgnoreCase));

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
                    Bidder = userBid.Bidder,
                    Amount = userBid.Amount,
                    BidTime = userBid.BidTime.ToString("O"),
                    Status = userBid.Status.ToString()
                };
            }

            return response;
        }
    }
}
