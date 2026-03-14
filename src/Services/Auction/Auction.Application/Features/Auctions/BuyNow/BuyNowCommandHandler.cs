using Auctions.Application.DTOs.Audit;
using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Auditing;
using Auctions.Domain.Enums;
using Auctions.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Auctions.BuyNow;

public class BuyNowCommandHandler : ICommandHandler<BuyNowCommand, BuyNowResultDto>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<BuyNowCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLock _distributedLock;
    private readonly IAuditPublisher _auditPublisher;

    public BuyNowCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<BuyNowCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IDistributedLock distributedLock,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _distributedLock = distributedLock;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<BuyNowResultDto>> Handle(BuyNowCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Buy Now for auction {AuctionId}", request.AuctionId);

        var lockKey = $"auction:buynow:{request.AuctionId}";
        await using var lockHandle = await _distributedLock.AcquireAsync(
            lockKey,
            expiry: TimeSpan.FromSeconds(AuctionDefaults.Lock.ExpirySeconds),
            wait: TimeSpan.FromSeconds(AuctionDefaults.Lock.WaitSeconds),
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Failed to acquire lock for BuyNow on auction {AuctionId}", request.AuctionId);
            return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.Conflict);
        }

        try
        {

            var auction = await _repository.GetByIdForUpdateAsync(request.AuctionId, cancellationToken);

            if (auction == null)
            {
                return Result.Failure<BuyNowResultDto>(AuctionErrors.Auction.NotFound);
            }

            if (!auction.IsBuyNowAvailable)
            {
                return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.NotAvailable);
            }

            if (auction.SellerId == request.BuyerId)
            {
                return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.OwnAuction);
            }

            if (auction.Status != Status.Live)
            {
                return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.AuctionNotLive);
            }

            var oldAuctionData = AuctionAuditData.FromAuction(auction);

            auction.ExecuteBuyNow(request.BuyerId, request.BuyerUsername);

            await _repository.UpdateAsync(auction, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditPublisher.PublishAsync(
                auction.Id,
                AuctionAuditData.FromAuction(auction),
                AuditAction.Updated,
                oldAuctionData,
                new Dictionary<string, object>
                {
                    ["Action"] = "BuyNow",
                    ["BuyerId"] = request.BuyerId,
                    ["BuyerUsername"] = request.BuyerUsername,
                    ["Price"] = auction.BuyNowPrice!.Value
                },
                cancellationToken);

            _logger.LogInformation("Buy Now completed for auction {AuctionId}. Price: {Price}",
                auction.Id, auction.BuyNowPrice);

            return Result<BuyNowResultDto>.Success(new BuyNowResultDto
            {
                AuctionId = auction.Id,
                Buyer = request.BuyerUsername,
                Seller = auction.SellerUsername,
                BuyNowPrice = auction.BuyNowPrice!.Value,
                ItemTitle = auction.Item.Title,
                Success = true
            });
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            _logger.LogWarning(ex, "Concurrency conflict in BuyNow for auction {AuctionId}", request.AuctionId);
            return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.ConflictPurchased);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Buy Now failed for auction {AuctionId}", request.AuctionId);
            return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.Failed("An unexpected error occurred. Please try again."));
        }
    }
}
