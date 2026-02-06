using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using MassTransit;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class ReleaseAuctionReservationConsumer : IConsumer<ReleaseAuctionReservation>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly BuildingBlocks.Application.Abstractions.IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<ReleaseAuctionReservationConsumer> _logger;

    public ReleaseAuctionReservationConsumer(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        BuildingBlocks.Application.Abstractions.IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<ReleaseAuctionReservationConsumer> logger)
    {
        _readRepository = readRepository;
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
            var auction = await _readRepository.GetByIdAsync(message.AuctionId);

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
                auction.ChangeStatus(Status.Live);
                await _writeRepository.UpdateAsync(auction, context.CancellationToken);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

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

