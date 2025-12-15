using AuctionService.API.Grpc;
using BidService.Application.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace BidService.API.Services;

public class AuctionValidationService : IAuctionValidationService
{
    private readonly AuctionGrpc.AuctionGrpcClient _auctionClient;
    private readonly ILogger<AuctionValidationService> _logger;

    public AuctionValidationService(
        AuctionGrpc.AuctionGrpcClient auctionClient,
        ILogger<AuctionValidationService> logger)
    {
        _auctionClient = auctionClient;
        _logger = logger;
    }

    public async Task<AuctionValidationResult> ValidateAuctionForBidAsync(
        Guid auctionId,
        string bidder,
        int bidAmount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ValidateAuctionRequest
            {
                AuctionId = auctionId.ToString(),
                Bidder = bidder,
                BidAmount = bidAmount
            };

            var response = await _auctionClient.ValidateAuctionForBidAsync(
                request,
                cancellationToken: cancellationToken);

            var result = new AuctionValidationResult
            {
                IsValid = response.IsValid,
                ErrorCode = response.ErrorCode,
                ErrorMessage = response.ErrorMessage,
                CurrentHighBid = response.CurrentHighBid,
                ReservePrice = response.ReservePrice,
                Seller = response.Seller,
                Status = response.Status
            };

            if (!string.IsNullOrEmpty(response.AuctionEnd) && 
                DateTimeOffset.TryParse(response.AuctionEnd, out var auctionEnd))
            {
                result.AuctionEnd = auctionEnd;
            }

            return result;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error validating auction {AuctionId}", auctionId);
            
            return new AuctionValidationResult
            {
                IsValid = false,
                ErrorCode = "SERVICE_UNAVAILABLE",
                ErrorMessage = "Unable to validate auction. Please try again."
            };
        }
    }

    public async Task<AuctionInfo?> GetAuctionDetailsAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetAuctionDetailsRequest
            {
                AuctionId = auctionId.ToString()
            };

            var response = await _auctionClient.GetAuctionDetailsAsync(
                request,
                cancellationToken: cancellationToken);

            return new AuctionInfo
            {
                Id = Guid.Parse(response.Id),
                Title = response.Title,
                Seller = response.Seller,
                Winner = string.IsNullOrEmpty(response.Winner) ? null : response.Winner,
                CurrentHighBid = response.CurrentHighBid,
                ReservePrice = response.ReservePrice,
                BuyNowPrice = response.BuyNowPrice > 0 ? response.BuyNowPrice : null,
                AuctionEnd = DateTimeOffset.Parse(response.AuctionEnd),
                Status = response.Status,
                IsBuyNowAvailable = response.IsBuyNowAvailable
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting auction details {AuctionId}", auctionId);
            throw;
        }
    }

    public async Task<bool> ExtendAuctionAsync(
        Guid auctionId,
        int extendMinutes,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ExtendAuctionRequest
            {
                AuctionId = auctionId.ToString(),
                ExtendMinutes = extendMinutes,
                Reason = reason
            };

            var response = await _auctionClient.ExtendAuctionAsync(
                request,
                cancellationToken: cancellationToken);

            if (response.Success)
            {
                _logger.LogInformation(
                    "Extended auction {AuctionId} to {NewEndTime}",
                    auctionId, response.NewEndTime);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to extend auction {AuctionId}: {Error}",
                    auctionId, response.ErrorMessage);
            }

            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error extending auction {AuctionId}", auctionId);
            return false;
        }
    }
}
