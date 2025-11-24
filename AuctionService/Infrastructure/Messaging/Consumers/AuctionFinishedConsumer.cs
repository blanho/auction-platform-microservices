using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Domain.Enums;

namespace AuctionService.Infrastructure.Messaging.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly IAuctionRepository _repository;
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(IAuctionRepository repository, ILogger<AuctionFinishedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming AuctionFinishedEvent for auction {AuctionId}", message.AuctionId);

        var auction = await _repository.GetByIdAsync(message.AuctionId);
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found", message.AuctionId);
            return;
        }

        if (message.ItemSold)
        {
            auction.Winner = message.Winner;
            auction.SoldAmount = message.SoldAmount;
        }
        
        auction.Status = message.SoldAmount > auction.ReversePrice
            ? Status.Finished
            : Status.ReservedNotMet;

        _logger.LogInformation("Auction {AuctionId} finished - sold to {Winner} for {Amount}",
            message.AuctionId, message.Winner, message.SoldAmount);

        await _repository.UpdateAsync(auction);
        
        _logger.LogInformation("Successfully updated auction {AuctionId} status to {Status}", 
            message.AuctionId, auction.Status);
    }
}
