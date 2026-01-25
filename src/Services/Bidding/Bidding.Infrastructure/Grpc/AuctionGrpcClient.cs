using Auctions.Api.Grpc;
using Bidding.Application.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Grpc;

public class AuctionGrpcClient : IAuctionGrpcClient
{
    private readonly AuctionGrpc.AuctionGrpcClient _client;
    private readonly ILogger<AuctionGrpcClient> _logger;

    public AuctionGrpcClient(
        AuctionGrpc.AuctionGrpcClient client,
        ILogger<AuctionGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<AuctionValidationResult> ValidateAuctionForBidAsync(
        Guid auctionId,
        string bidderUsername,
        decimal bidAmount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ValidateAuctionRequest
            {
                AuctionId = auctionId.ToString(),
                Bidder = bidderUsername,
                BidAmountCents = DecimalToCents(bidAmount)
            };

            var response = await _client.ValidateAuctionForBidAsync(request, cancellationToken: cancellationToken);

            return new AuctionValidationResult(
                response.IsValid,
                response.ErrorMessage,
                response.ErrorCode);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            _logger.LogError(ex, "Auction service unavailable during bid validation for {AuctionId}", auctionId);
            return new AuctionValidationResult(false, "Auction service temporarily unavailable", "SERVICE_UNAVAILABLE");
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error validating auction {AuctionId}: {Status}", auctionId, ex.Status);
            return new AuctionValidationResult(false, "Failed to validate auction", "VALIDATION_ERROR");
        }
    }

    public async Task<AuctionDetails?> GetAuctionDetailsAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetAuctionDetailsRequest { AuctionId = auctionId.ToString() };
            var response = await _client.GetAuctionDetailsAsync(request, cancellationToken: cancellationToken);

            return new AuctionDetails(
                response.Title,
                response.Seller,
                DateTime.Parse(response.AuctionEnd),
                response.Status,
                response.IsBuyNowAvailable);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning("Auction {AuctionId} not found", auctionId);
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get auction details for {AuctionId}", auctionId);
            return null;
        }
    }

    public async Task<ExtendAuctionResult> ExtendAuctionAsync(
        Guid auctionId,
        DateTime newEndTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timeToExtend = (int)(newEndTime - DateTime.UtcNow).TotalMinutes;
            var request = new ExtendAuctionRequest
            {
                AuctionId = auctionId.ToString(),
                ExtendMinutes = timeToExtend > 0 ? timeToExtend : 10,
                Reason = "Anti-snipe protection"
            };

            var response = await _client.ExtendAuctionAsync(request, cancellationToken: cancellationToken);

            DateTime? parsedEndTime = null;
            if (!string.IsNullOrEmpty(response.NewEndTime) && DateTime.TryParse(response.NewEndTime, out var dt))
            {
                parsedEndTime = dt;
            }

            return new ExtendAuctionResult(response.Success, response.ErrorMessage ?? string.Empty, parsedEndTime);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to extend auction {AuctionId}", auctionId);
            return new ExtendAuctionResult(false, "Failed to extend auction");
        }
    }

    private static long DecimalToCents(decimal amount)
    {
        return (long)decimal.Round(amount * 100, MidpointRounding.AwayFromZero);
    }
}
