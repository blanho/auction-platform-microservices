using Auctions.Domain.Enums;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using OrchestrationService.Contracts.Events;
using MassTransit;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class CompleteBuyNowAuctionConsumer : IConsumer<CompleteBuyNowAuction>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CompleteBuyNowAuctionConsumer> _logger;

    public CompleteBuyNowAuctionConsumer(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<CompleteBuyNowAuctionConsumer> logger)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CompleteBuyNowAuction> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Completing Buy Now auction - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}, OrderId: {OrderId}",
            message.CorrelationId, message.AuctionId, message.OrderId);

        try
        {
            var auction = await _readRepository.GetByIdAsync(message.AuctionId, context.CancellationToken);

            if (auction == null)
            {
                _logger.LogError(
                    "Auction not found for completion - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                    message.CorrelationId, message.AuctionId);

                await context.Publish(new BuyNowAuctionCompletionFailed
                {
                    CorrelationId = message.CorrelationId,
                    AuctionId = message.AuctionId,
                    Reason = "Auction not found"
                });
                return;
            }

            auction.ExecuteBuyNow(message.BuyerId, message.BuyerUsername);
            await _writeRepository.UpdateAsync(auction, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Buy Now auction completed - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                message.CorrelationId, message.AuctionId);

            await context.Publish(new BuyNowAuctionCompleted
            {
                CorrelationId = message.CorrelationId,
                AuctionId = message.AuctionId,
                OrderId = message.OrderId,
                CompletedAt = _dateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to complete Buy Now auction - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                message.CorrelationId, message.AuctionId);

            await context.Publish(new BuyNowAuctionCompletionFailed
            {
                CorrelationId = message.CorrelationId,
                AuctionId = message.AuctionId,
                Reason = $"Failed to complete Buy Now auction: {ex.Message}"
            });
        }
    }
}

