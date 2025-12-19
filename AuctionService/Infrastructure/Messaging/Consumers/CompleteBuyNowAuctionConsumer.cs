using AuctionService.Application.Interfaces;
using Common.Core.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Events.Saga;
using Common.Repository.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.Messaging.Consumers;

public class CompleteBuyNowAuctionConsumer : IConsumer<CompleteBuyNowAuction>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CompleteBuyNowAuctionConsumer> _logger;

    public CompleteBuyNowAuctionConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<CompleteBuyNowAuctionConsumer> logger)
    {
        _repository = repository;
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
            var auction = await _repository.GetByIdAsync(message.AuctionId);

            if (auction == null)
            {
                _logger.LogError(
                    "Auction not found for completion - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                    message.CorrelationId, message.AuctionId);
                return;
            }

            auction.ExecuteBuyNow(message.BuyerId, message.BuyerUsername);
            await _repository.UpdateAsync(auction);
            await _unitOfWork.SaveChangesAsync();

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
            throw;
        }
    }
}
