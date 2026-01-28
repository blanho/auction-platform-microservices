using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Locking;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Application.Commands.BuyNow;

public class BuyNowCommandHandler : ICommandHandler<BuyNowCommand, BuyNowResultDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<BuyNowCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLock _distributedLock;

    public BuyNowCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<BuyNowCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        IDistributedLock distributedLock)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _distributedLock = distributedLock;
    }

    public async Task<Result<BuyNowResultDto>> Handle(BuyNowCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Buy Now for auction {AuctionId} by buyer {Buyer}",
            request.AuctionId, request.BuyerUsername);

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
            var auction = await _repository.GetByIdAsync(request.AuctionId, cancellationToken);

            if (auction == null)
            {
                return Result.Failure<BuyNowResultDto>(AuctionErrors.Auction.NotFound);
            }

            if (!auction.IsBuyNowAvailable)
            {
                return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.NotAvailable);
            }

            if (auction.SellerUsername == request.BuyerUsername)
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

            _logger.LogInformation("Buy Now completed for auction {AuctionId}. Buyer: {Buyer}, Price: {Price}",
                auction.Id, request.BuyerUsername, auction.BuyNowPrice);

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
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning("Concurrency conflict in BuyNow for auction {AuctionId}: {Error}",
                request.AuctionId, ex.Message);
            return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.ConflictPurchased);
        }
        catch (Exception ex)
        {
            _logger.LogError("Buy Now failed for auction {AuctionId}: {Error}", request.AuctionId, ex.Message);
            return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.Failed(ex.Message));
        }
    }
}
