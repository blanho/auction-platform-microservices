using AuctionService.API.Grpc;
using AuctionService.Application.Interfaces;
using Grpc.Core;
using AuctionStatus = Common.Domain.Enums.Status;

namespace AuctionService.API.Services;

public class AuctionGrpcService : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly ILogger<AuctionGrpcService> _logger;

    public AuctionGrpcService(
        IAuctionRepository auctionRepository,
        ILogger<AuctionGrpcService> logger)
    {
        _auctionRepository = auctionRepository;
        _logger = logger;
    }

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
            CurrentHighBid = (int)(auction.CurrentHighBid ?? 0),
            ReservePrice = (int)auction.ReservePrice,
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
            CurrentHighBid = (int)(auction.CurrentHighBid ?? 0),
            ReservePrice = (int)auction.ReservePrice,
            BuyNowPrice = (int)(auction.BuyNowPrice ?? 0),
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

        auction.AuctionEnd = auction.AuctionEnd.AddMinutes(request.ExtendMinutes);
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

    public override async Task<AuctionStatsResponse> GetAuctionStats(
        GetAuctionStatsRequest request,
        ServerCallContext context)
    {
        var liveCount = await _auctionRepository.GetCountByStatusAsync(AuctionStatus.Live, context.CancellationToken);
        var finishedCount = await _auctionRepository.GetCountByStatusAsync(AuctionStatus.Finished, context.CancellationToken);
        var totalRevenue = await _auctionRepository.GetTotalRevenueAsync(context.CancellationToken);
        var totalCount = await _auctionRepository.GetTotalCountAsync(context.CancellationToken);

        return new AuctionStatsResponse
        {
            LiveAuctions = liveCount,
            TotalAuctions = totalCount,
            CompletedAuctions = finishedCount,
            TotalRevenue = (double)totalRevenue
        };
    }

    public override async Task<AuctionAnalyticsResponse> GetAuctionAnalytics(
        GetAuctionAnalyticsRequest request,
        ServerCallContext context)
    {
        var liveCount = await _auctionRepository.GetCountByStatusAsync(AuctionStatus.Live, context.CancellationToken);
        var finishedCount = await _auctionRepository.GetCountByStatusAsync(AuctionStatus.Finished, context.CancellationToken);
        var totalRevenue = await _auctionRepository.GetTotalRevenueAsync(context.CancellationToken);
        var totalCount = await _auctionRepository.GetTotalCountAsync(context.CancellationToken);

        var today = DateTimeOffset.UtcNow.Date;
        var endOfWeek = today.AddDays(7);

        var endingToday = await _auctionRepository.GetCountEndingBetweenAsync(
            today, today.AddDays(1), context.CancellationToken);
        var endingThisWeek = await _auctionRepository.GetCountEndingBetweenAsync(
            today, endOfWeek, context.CancellationToken);

        var successRate = totalCount > 0 ? (double)finishedCount / totalCount * 100 : 0;
        var averagePrice = finishedCount > 0 ? (double)totalRevenue / finishedCount : 0;

        return new AuctionAnalyticsResponse
        {
            LiveAuctions = liveCount,
            CompletedAuctions = finishedCount,
            CancelledAuctions = 0,
            PendingAuctions = 0,
            TotalRevenue = (double)totalRevenue,
            AverageFinalPrice = averagePrice,
            SuccessRate = successRate,
            AuctionsEndingToday = endingToday,
            AuctionsEndingThisWeek = endingThisWeek
        };
    }

    public override async Task<TopAuctionsResponse> GetTopAuctions(
        GetTopAuctionsRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit > 0 ? request.Limit : 10;
        var topAuctions = await _auctionRepository.GetTopByRevenueAsync(limit, context.CancellationToken);

        var response = new TopAuctionsResponse();
        foreach (var auction in topAuctions)
        {
            response.Auctions.Add(new TopAuction
            {
                AuctionId = auction.Id.ToString(),
                Title = auction.Item?.Title ?? string.Empty,
                SellerUsername = auction.SellerUsername,
                FinalPrice = (double)(auction.SoldAmount ?? auction.CurrentHighBid ?? 0),
                BidCount = 0
            });
        }

        return response;
    }

    public override async Task<CategoryStatsResponse> GetCategoryStats(
        GetCategoryStatsRequest request,
        ServerCallContext context)
    {
        var categoryStats = await _auctionRepository.GetCategoryStatsAsync(context.CancellationToken);
        var totalRevenue = categoryStats.Sum(c => c.Revenue);

        var response = new CategoryStatsResponse();
        foreach (var stat in categoryStats)
        {
            response.Categories.Add(new CategoryStat
            {
                CategoryId = stat.CategoryId.ToString(),
                CategoryName = stat.CategoryName,
                AuctionCount = stat.AuctionCount,
                Revenue = (double)stat.Revenue,
                Percentage = totalRevenue > 0 ? (double)(stat.Revenue / totalRevenue * 100) : 0
            });
        }

        return response;
    }
}
