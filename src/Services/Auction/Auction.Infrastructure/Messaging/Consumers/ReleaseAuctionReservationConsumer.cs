using Auctions.Domain.Enums;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ReleaseAuctionReservationConsumer : IConsumer<ReleaseAuctionReservation>
{
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<ReleaseAuctionReservationConsumer> _logger;

    public ReleaseAuctionReservationConsumer(
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<ReleaseAuctionReservationConsumer> logger)
    {
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReleaseAuctionReservation> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Releasing auction reservation - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}, Reason: {Reason}",
            message.CorrelationId, message.AuctionId, message.Reason);

        try
        {
            var auction = await _writeRepository.GetByIdForUpdateAsync(message.AuctionId, context.CancellationToken);

            if (auction == null)
            {
                _logger.LogWarning(
                    "Auction not found for reservation release - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                    message.CorrelationId, message.AuctionId);

                await context.Publish(new AuctionReservationReleased
                {
                    CorrelationId = message.CorrelationId,
                    AuctionId = message.AuctionId,
                    ReleasedAt = _dateTime.UtcNow
                });
                return;
            }

            // Completion won the race: preserve the sale and report its actual outcome.
            if (auction.BuyNowCorrelationId == message.CorrelationId && auction.BuyNowOrderId.HasValue)
            {
                await context.Publish(new BuyNowAuctionCompleted
                {
                    CorrelationId = message.CorrelationId,
                    AuctionId = message.AuctionId,
                    OrderId = auction.BuyNowOrderId.Value,
                    CompletedAt = _dateTime.UtcNow
                });
                return;
            }

            auction.ReleaseBuyNow(message.CorrelationId);
            await _writeRepository.UpdateAsync(auction, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            await context.Publish(new AuctionReservationReleased
            {
                CorrelationId = message.CorrelationId,
                AuctionId = message.AuctionId,
                ReleasedAt = _dateTime.UtcNow
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Failed to release auction reservation - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                message.CorrelationId, message.AuctionId);
            throw;
        }
    }
}
