using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions.Locking;
using Auctions.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.BuyNow;

public class BuyNowCommandHandler : ICommandHandler<BuyNowCommand, BuyNowResultDto>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<BuyNowCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLock _distributedLock;

    public BuyNowCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<BuyNowCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IDistributedLock distributedLock)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _distributedLock = distributedLock;
    }

    public async Task<Result<BuyNowResultDto>> Handle(BuyNowCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Buy Now for auction {AuctionId}", request.AuctionId);

        var lockKey = $"auction:buynow:{request.AuctionId}";
        await using var lockHandle = await _distributedLock.AcquireAsync(
            lockKey,
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(5),
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

            auction.ExecuteBuyNow(request.BuyerId, request.BuyerUsername);

            await _repository.UpdateAsync(auction, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
