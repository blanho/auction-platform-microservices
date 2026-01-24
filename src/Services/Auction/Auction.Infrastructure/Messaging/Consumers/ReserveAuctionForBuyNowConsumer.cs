using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ReserveAuctionForBuyNowConsumer : IConsumer<ReserveAuctionForBuyNow>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<ReserveAuctionForBuyNowConsumer> _logger;

    public ReserveAuctionForBuyNowConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<ReserveAuctionForBuyNowConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReserveAuctionForBuyNow> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Processing ReserveAuctionForBuyNow - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
            message.CorrelationId, message.AuctionId);

        try
        {
            var auction = await _repository.GetByIdAsync(message.AuctionId);

            if (auction == null)
            {
                await context.Publish(new AuctionReservationFailed
                {
                    CorrelationId = message.CorrelationId,
                    AuctionId = message.AuctionId,
                    Reason = "Auction not found",
                    FailedAt = _dateTime.UtcNow
                });
                return;
            }

            if (!auction.IsBuyNowAvailable)
            {
                await context.Publish(new AuctionReservationFailed
                {
                    CorrelationId = message.CorrelationId,
                    AuctionId = message.AuctionId,
                    Reason = "Buy Now is not available for this auction",
                    FailedAt = _dateTime.UtcNow
                });
                return;
            }

            if (auction.SellerUsername == message.BuyerUsername)
            {
                await context.Publish(new AuctionReservationFailed
                {
                    CorrelationId = message.CorrelationId,
                    AuctionId = message.AuctionId,
                    Reason = "Cannot buy your own auction",
                    FailedAt = _dateTime.UtcNow
                });
                return;
            }

            if (auction.Status != Status.Live)
            {
                await context.Publish(new AuctionReservationFailed
                {
                    CorrelationId = message.CorrelationId,
                    AuctionId = message.AuctionId,
                    Reason = "Auction is no longer active",
                    FailedAt = _dateTime.UtcNow
                });
                return;
            }

            auction.ChangeStatus(Status.ReservedForBuyNow);
            await _repository.UpdateAsync(auction, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Auction {AuctionId} reserved for Buy Now - CorrelationId: {CorrelationId}",
                message.AuctionId, message.CorrelationId);

            await context.Publish(new AuctionReservedForBuyNow
            {
                CorrelationId = message.CorrelationId,
                AuctionId = auction.Id,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                BuyNowPrice = auction.BuyNowPrice ?? 0,
                ItemTitle = auction.Item.Title,
                ReservedAt = _dateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to reserve auction {AuctionId} for Buy Now - CorrelationId: {CorrelationId}",
                message.AuctionId, message.CorrelationId);

            await context.Publish(new AuctionReservationFailed
            {
                CorrelationId = message.CorrelationId,
                AuctionId = message.AuctionId,
                Reason = $"Failed to reserve auction: {ex.Message}",
                FailedAt = _dateTime.UtcNow
            });
        }
    }
}

