using Auctions.Api.Grpc;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService
{
    public override async Task<ValidateAuctionResponse> ValidateAuctionForBid(
        ValidateAuctionRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "Validating auction {AuctionId} for bid by {Bidder}, amount: {Amount}",
            request.AuctionId, request.Bidder, request.BidAmount);

        if (!Guid.TryParse(request.AuctionId, out var auctionId))
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "INVALID_AUCTION_ID",
                ErrorMessage = "Invalid auction ID format"
            };
        }

        var auction = await _auctionRepository.GetByIdAsync(auctionId, context.CancellationToken);

        if (auction == null)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_NOT_FOUND",
                ErrorMessage = "Auction not found"
            };
        }

        if (auction.Status != AuctionStatus.Live)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_NOT_LIVE",
                ErrorMessage = $"Auction is not active. Current status: {auction.Status}",
                Status = auction.Status.ToString()
            };
        }

        if (auction.AuctionEnd < DateTimeOffset.UtcNow)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_ENDED",
                ErrorMessage = "Auction has ended",
                AuctionEnd = auction.AuctionEnd.ToString("O")
            };
        }

        if (auction.SellerUsername.Equals(request.Bidder, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "SELLER_CANNOT_BID",
                ErrorMessage = "You cannot bid on your own auction",
                Seller = auction.SellerUsername
            };
        }

        if (auction.SoldAmount.HasValue)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_SOLD",
                ErrorMessage = "This auction has already been sold"
            };
        }

        return new ValidateAuctionResponse
        {
            IsValid = true,
            CurrentHighBid = (int)((auction.CurrentHighBid ?? 0) * 100),
            ReservePrice = (int)(auction.ReservePrice * 100),
            AuctionEnd = auction.AuctionEnd.ToString("O"),
            Seller = auction.SellerUsername,
            Status = auction.Status.ToString()
        };
    }

    public override async Task<AuctionDetailsResponse> GetAuctionDetails(
        GetAuctionDetailsRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.AuctionId, out var auctionId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid auction ID"));
        }

        var auction = await _auctionRepository.GetByIdAsync(auctionId, context.CancellationToken);

        if (auction == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));
        }

        return new AuctionDetailsResponse
        {
            Id = auction.Id.ToString(),
            Title = auction.Item?.Title ?? string.Empty,
            Seller = auction.SellerUsername,
            Winner = auction.WinnerUsername ?? string.Empty,
            CurrentHighBid = (int)((auction.CurrentHighBid ?? 0) * 100),
            ReservePrice = (int)(auction.ReservePrice * 100),
            BuyNowPrice = (int)((auction.BuyNowPrice ?? 0) * 100),
            AuctionEnd = auction.AuctionEnd.ToString("O"),
            Status = auction.Status.ToString(),
            IsBuyNowAvailable = auction.IsBuyNowAvailable
        };
    }

    public override async Task<ExtendAuctionResponse> ExtendAuction(
        ExtendAuctionRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "Extending auction {AuctionId} by {Minutes} minutes. Reason: {Reason}",
            request.AuctionId, request.ExtendMinutes, request.Reason);

        if (!Guid.TryParse(request.AuctionId, out var auctionId))
        {
            return new ExtendAuctionResponse
            {
                Success = false,
                ErrorMessage = "Invalid auction ID"
            };
        }

        var auction = await _auctionRepository.GetByIdAsync(auctionId, context.CancellationToken);

        if (auction == null)
        {
            return new ExtendAuctionResponse
            {
                Success = false,
                ErrorMessage = "Auction not found"
            };
        }

        if (auction.Status != AuctionStatus.Live)
        {
            return new ExtendAuctionResponse
            {
                Success = false,
                ErrorMessage = "Can only extend live auctions"
            };
        }

        auction.ExtendAuctionEnd(TimeSpan.FromMinutes(request.ExtendMinutes), "gRPC extension request");
        await _auctionRepository.UpdateAsync(auction, context.CancellationToken);

        _logger.LogInformation(
            "Extended auction {AuctionId} to {NewEndTime}",
            auctionId, auction.AuctionEnd);

        return new ExtendAuctionResponse
        {
            Success = true,
            NewEndTime = auction.AuctionEnd.ToString("O")
        };
    }
}
