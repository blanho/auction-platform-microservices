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
        _logger.LogDebug(
            "Validating auction {AuctionId} for bid, amount cents: {AmountCents}",
            request.AuctionId, request.BidAmountCents);

        if (!Guid.TryParse(request.AuctionId, out var auctionId))
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "INVALID_AUCTION_ID",
                ErrorMessage = _localization.GetString("Grpc.InvalidAuctionId")
            };
        }

        var auction = await _readRepository.GetByIdAsync(auctionId, context.CancellationToken);

        if (auction == null)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_NOT_FOUND",
                ErrorMessage = _localization.GetString("Grpc.AuctionNotFound")
            };
        }

        if (auction.Status != AuctionStatus.Live)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_NOT_LIVE",
                ErrorMessage = _localization.GetString("Grpc.AuctionNotActive", auction.Status),
                Status = auction.Status.ToString()
            };
        }

        if (auction.AuctionEnd < DateTimeOffset.UtcNow)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_ENDED",
                ErrorMessage = _localization.GetString("Grpc.AuctionEnded"),
                AuctionEnd = auction.AuctionEnd.ToString("O")
            };
        }

        if (auction.SellerUsername.Equals(request.Bidder, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "SELLER_CANNOT_BID",
                ErrorMessage = _localization.GetString("Grpc.CannotBidOwnAuction"),
                Seller = auction.SellerUsername
            };
        }

        if (auction.SoldAmount.HasValue)
        {
            return new ValidateAuctionResponse
            {
                IsValid = false,
                ErrorCode = "AUCTION_SOLD",
                ErrorMessage = _localization.GetString("Grpc.AuctionSold")
            };
        }

        return new ValidateAuctionResponse
        {
            IsValid = true,
            CurrentHighBidCents = DecimalToCents(auction.CurrentHighBid ?? 0),
            ReservePriceCents = DecimalToCents(auction.ReservePrice),
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
            throw new RpcException(new Status(StatusCode.InvalidArgument, _localization.GetString("Grpc.InvalidAuctionId")));
        }

        var auction = await _readRepository.GetByIdAsync(auctionId, context.CancellationToken);

        if (auction == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, _localization.GetString("Grpc.AuctionNotFound")));
        }

        return new AuctionDetailsResponse
        {
            Id = auction.Id.ToString(),
            Title = auction.Item?.Title ?? string.Empty,
            Seller = auction.SellerUsername,
            Winner = auction.WinnerUsername ?? string.Empty,
            CurrentHighBidCents = DecimalToCents(auction.CurrentHighBid ?? 0),
            ReservePriceCents = DecimalToCents(auction.ReservePrice),
            BuyNowPriceCents = DecimalToCents(auction.BuyNowPrice ?? 0),
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
                ErrorMessage = _localization.GetString("Grpc.InvalidAuctionId")
            };
        }

        var auction = await _readRepository.GetByIdAsync(auctionId, context.CancellationToken);

        if (auction == null)
        {
            return new ExtendAuctionResponse
            {
                Success = false,
                ErrorMessage = _localization.GetString("Grpc.AuctionNotFound")
            };
        }

        if (auction.Status != AuctionStatus.Live)
        {
            return new ExtendAuctionResponse
            {
                Success = false,
                ErrorMessage = _localization.GetString("Grpc.CanOnlyExtendLive")
            };
        }

        auction.ExtendAuctionEnd(TimeSpan.FromMinutes(request.ExtendMinutes));
        await _auctionWriteRepository.UpdateAsync(auction, context.CancellationToken);

        _logger.LogInformation(
            "Extended auction {AuctionId} to {NewEndTime}",
            auctionId, auction.AuctionEnd);

        return new ExtendAuctionResponse
        {
            Success = true,
            NewEndTime = auction.AuctionEnd.ToString("O")
        };
    }

    private static long DecimalToCents(decimal amount)
    {
        return (long)decimal.Round(amount * 100, MidpointRounding.AwayFromZero);
    }
}
