using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidService.Infrastructure.Messaging.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(ILogger<AuctionFinishedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming AuctionFinishedEvent for auction {AuctionId}, ItemSold: {ItemSold}",
            message.AuctionId,
            message.ItemSold);

        if (message.ItemSold)
        {
            _logger.LogInformation(
                "Auction {AuctionId} finished - sold to {Winner} for {Amount}",
                message.AuctionId,
                message.WinnerUsername,
                message.SoldAmount);
        }
        else
        {
            _logger.LogInformation(
                "Auction {AuctionId} finished - reserve price not met",
                message.AuctionId);
        }
    }
}
