using BidService.API.Grpc;
using Grpc.Core;

namespace AuctionService.API.Services
{
    public interface IBidGrpcClient
    {
        Task<BidResponse?> GetHighestBidAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<BidListResponse> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<HasUserBidResponse> HasUserBidOnAuctionAsync(Guid auctionId, string bidder, CancellationToken cancellationToken = default);
    }

    public class BidGrpcClient : IBidGrpcClient
    {
        private readonly BidGrpc.BidGrpcClient _client;
        private readonly ILogger<BidGrpcClient> _logger;

        public BidGrpcClient(BidGrpc.BidGrpcClient client, ILogger<BidGrpcClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<BidResponse?> GetHighestBidAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling BidService gRPC GetHighestBid for auction {AuctionId}", auctionId);
                
                var request = new GetHighestBidRequest
                {
                    AuctionId = auctionId.ToString()
                };

                var response = await _client.GetHighestBidAsync(request, cancellationToken: cancellationToken);
                if (string.IsNullOrEmpty(response.Id))
                {
                    return null;
                }

                return response;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                _logger.LogInformation("No bids found for auction {AuctionId}", auctionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BidService gRPC for auction {AuctionId}", auctionId);
                throw;
            }
        }

        public async Task<BidListResponse> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling BidService gRPC GetBidsForAuction for auction {AuctionId}", auctionId);
                
                var request = new GetBidsForAuctionRequest
                {
                    AuctionId = auctionId.ToString()
                };

                return await _client.GetBidsForAuctionAsync(request, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BidService gRPC for auction {AuctionId}", auctionId);
                throw;
            }
        }

        public async Task<HasUserBidResponse> HasUserBidOnAuctionAsync(Guid auctionId, string bidder, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling BidService gRPC HasUserBidOnAuction for auction {AuctionId} and bidder {Bidder}", 
                    auctionId, bidder);
                
                var request = new HasUserBidRequest
                {
                    AuctionId = auctionId.ToString(),
                    Bidder = bidder
                };

                return await _client.HasUserBidOnAuctionAsync(request, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BidService gRPC for auction {AuctionId} and bidder {Bidder}", 
                    auctionId, bidder);
                throw;
            }
        }
    }
}
