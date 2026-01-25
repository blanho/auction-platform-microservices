using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Locking;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Auctions.Application.Commands.BuyNow;

public class BuyNowCommandHandler : ICommandHandler<BuyNowCommand, BuyNowResultDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<BuyNowCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IDistributedLock _distributedLock;

    public BuyNowCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<BuyNowCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher,
        IDistributedLock distributedLock)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
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
            return Result.Failure<BuyNowResultDto>(
                Error.Create("BuyNow.Conflict", "Another buyer is currently processing this purchase. Please try again."));
        }

        try
        {
            var auction = await _repository.GetByIdAsync(request.AuctionId, cancellationToken);

            if (auction == null)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("Auction.NotFound", "Auction not found"));
            }

            if (!auction.IsBuyNowAvailable)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("BuyNow.NotAvailable", "Buy Now is not available for this auction"));
            }

            if (auction.SellerUsername == request.BuyerUsername)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("BuyNow.OwnAuction", "You cannot buy your own auction"));
            }

            if (auction.Status != Status.Live)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("BuyNow.AuctionNotLive", "This auction is no longer active"));
            }

            auction.ExecuteBuyNow(request.BuyerId, request.BuyerUsername);

            await _repository.UpdateAsync(auction, cancellationToken);

            await _auditPublisher.PublishAsync(
                auction.Id,
                auction,
                AuditAction.Updated,
                cancellationToken: cancellationToken);

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
            return Result.Failure<BuyNowResultDto>(
                Error.Create("BuyNow.Conflict", "This item was just purchased by someone else. Please try another auction."));
        }
        catch (Exception ex)
        {
            _logger.LogError("Buy Now failed for auction {AuctionId}: {Error}", request.AuctionId, ex.Message);
            return Result.Failure<BuyNowResultDto>(
                Error.Create("BuyNow.Failed", $"Failed to process Buy Now: {ex.Message}"));
        }
    }
}
