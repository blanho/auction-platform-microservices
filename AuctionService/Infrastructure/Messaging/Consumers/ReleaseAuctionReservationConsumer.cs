using AuctionService.Application.Interfaces;
using Common.Core.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Events.Saga;
using Common.Repository.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.Messaging.Consumers;

public class ReleaseAuctionReservationConsumer : IConsumer<ReleaseAuctionReservation>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<ReleaseAuctionReservationConsumer> _logger;

    public ReleaseAuctionReservationConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<ReleaseAuctionReservationConsumer> logger)
    {
        _repository = repository;
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
            var auction = await _repository.GetByIdAsync(message.AuctionId);

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

            if (auction.Status == Status.ReservedForBuyNow)
            {
                auction.Status = Status.Live;
                await _repository.UpdateAsync(auction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Auction reservation released, status set back to Live - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                    message.CorrelationId, message.AuctionId);
            }
            else
            {
                _logger.LogWarning(
                    "Auction not in reserved state, skipping release - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}, CurrentStatus: {Status}",
                    message.CorrelationId, message.AuctionId, auction.Status);
            }

            await context.Publish(new AuctionReservationReleased
            {
                CorrelationId = message.CorrelationId,
                AuctionId = message.AuctionId,
                ReleasedAt = _dateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to release auction reservation - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                message.CorrelationId, message.AuctionId);
            throw;
        }
    }
}
