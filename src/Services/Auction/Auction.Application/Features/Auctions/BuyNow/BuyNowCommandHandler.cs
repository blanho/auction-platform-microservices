using BuildingBlocks.Application.Abstractions.Messaging;
using OrchestrationService.Contracts.Events;
using Auctions.Domain.Entities;
using DomainConcurrencyException = BuildingBlocks.Domain.Exceptions.ConcurrencyException;
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
    private readonly IBuyNowPurchaseRepository _purchases;
    private readonly IEventPublisher _publisher;
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
        IAuditPublisher auditPublisher,
        IBuyNowPurchaseRepository purchases,
        IEventPublisher publisher)
    {
        _purchases = purchases;
        _publisher = publisher;
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _distributedLock = distributedLock;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<BuyNowResultDto>> Handle(BuyNowCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Buy Now for auction {AuctionId}", request.AuctionId);

        var lockKey = AuctionDefaults.Lock.GetAuctionBuyNowKey(request.AuctionId);
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

            var existingId = request.CorrelationId ?? (auction.BuyNowBuyerId == request.BuyerId ? auction.BuyNowCorrelationId : null);
            if (existingId.HasValue)
            {
                var existing = await _purchases.GetAsync(existingId.Value, cancellationToken);
                if (existing is not null)
                    return existing.BuyerId == request.BuyerId && existing.AuctionId == request.AuctionId
                        ? Result<BuyNowResultDto>.Success(BuyNowResultDto.FromPurchase(existing))
                        : Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.Conflict);
            }

            if (auction.BuyNowBuyerId == request.BuyerId && auction.BuyNowCorrelationId.HasValue)
            {
                var active = await _purchases.GetAsync(auction.BuyNowCorrelationId.Value, cancellationToken);
                if (active is not null) return Result<BuyNowResultDto>.Success(BuyNowResultDto.FromPurchase(active));
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

            var correlationId = request.CorrelationId ?? Guid.NewGuid();
            auction.ReserveBuyNow(correlationId, request.BuyerId);
            var purchase = new BuyNowPurchase
            {
                CorrelationId = correlationId,
                AuctionId = auction.Id,
                BuyerId = request.BuyerId,
                Buyer = request.BuyerUsername,
                Seller = auction.SellerUsername,
                BuyNowPrice = auction.BuyNowPrice!.Value,
                ItemTitle = auction.Item.Title
            };
            await _purchases.AddAsync(purchase, cancellationToken);
            await _publisher.PublishAsync(new BuyNowSagaStarted
            {
                CorrelationId = correlationId,
                AuctionId = auction.Id,
                BuyerId = request.BuyerId,
                BuyerUsername = request.BuyerUsername,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                BuyNowPrice = auction.BuyNowPrice.Value,
                ItemTitle = auction.Item.Title,
                StartedAt = purchase.CreatedAt
            }, cancellationToken);

            await _repository.UpdateAsync(auction, cancellationToken);

            await _auditPublisher.PublishAsync(
                auction.Id,
                AuctionAuditData.FromAuction(auction),
                AuditAction.Updated,
                oldAuctionData,
                AuctionAuditMetadata.ForBuyNow(
                    request.BuyerId,
                    request.BuyerUsername,
                    auction.BuyNowPrice!.Value),
                cancellationToken);

            // Reservation, purchase receipt, saga start, and audit share the bus-outbox commit.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Buy Now purchase {CorrelationId} accepted for auction {AuctionId}", correlationId, auction.Id);
            return Result<BuyNowResultDto>.Success(BuyNowResultDto.FromPurchase(purchase));
        }
        catch (DomainConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict in BuyNow for auction {AuctionId}", request.AuctionId);
            return Result.Failure<BuyNowResultDto>(AuctionErrors.BuyNow.ConflictPurchased);
        }
    }
}
