using BidService.Contracts.Events;
using Bidding.Application.Interfaces;
using MassTransit;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class ProcessAutoBidsConsumer : IConsumer<ProcessAutoBidsEvent>
{
    private readonly IAutoBidService _autoBidService;
    private readonly ILogger<ProcessAutoBidsConsumer> _logger;

    public ProcessAutoBidsConsumer(
        IAutoBidService autoBidService,
        ILogger<ProcessAutoBidsConsumer> logger)
    {
        _autoBidService = autoBidService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessAutoBidsEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing auto-bids for auction {AuctionId} at current high bid {CurrentHighBid}",
            message.AuctionId,
            message.CurrentHighBid);

        await _autoBidService.ProcessAutoBidsForAuctionAsync(
            message.AuctionId,
            message.CurrentHighBid,
            context.CancellationToken);

        _logger.LogInformation(
            "Successfully processed auto-bids for auction {AuctionId}",
            message.AuctionId);

    }
}

public class ProcessAutoBidsConsumerDefinition : ConsumerDefinition<ProcessAutoBidsConsumer>
{
    public ProcessAutoBidsConsumerDefinition()
    {

        ConcurrentMessageLimit = 10;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProcessAutoBidsConsumer> consumerConfigurator,
        IRegistrationContext context)
    {

        endpointConfigurator.UseMessageRetry(r => r
            .Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(5)));

        endpointConfigurator.UseInMemoryOutbox(context);
    }
}
