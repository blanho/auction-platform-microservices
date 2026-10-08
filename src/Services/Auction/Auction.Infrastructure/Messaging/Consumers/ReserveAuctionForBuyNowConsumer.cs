using BuildingBlocks.Domain.Exceptions;
using Auctions.Domain.Enums;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ReserveAuctionForBuyNowConsumer : IConsumer<ReserveAuctionForBuyNow>
{
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<ReserveAuctionForBuyNowConsumer> _logger;

    public ReserveAuctionForBuyNowConsumer(
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<ReserveAuctionForBuyNowConsumer> logger)
    {
        _writeRepository = writeRepository;
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
            var auction = await _writeRepository.GetByIdForUpdateAsync(message.AuctionId, context.CancellationToken);

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

            auction.ReserveBuyNow(message.CorrelationId, message.BuyerId);
            await _writeRepository.UpdateAsync(auction, context.CancellationToken);
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
        catch (DomainInvariantException ex)
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
