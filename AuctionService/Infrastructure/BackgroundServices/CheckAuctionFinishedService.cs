using AuctionService.Application.Interfaces;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.BackgroundServices;

public class CheckAuctionFinishedService : BackgroundService
{
    private readonly ILogger<CheckAuctionFinishedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CheckAuctionFinishedService(
        ILogger<CheckAuctionFinishedService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CheckAuctionFinishedService is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctionsAsync(stoppingToken);
            
            // Check every 5 seconds
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task CheckAuctionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        
        try
        {
            var finishedAuctions = await repository.GetFinishedAuctionsAsync(cancellationToken);

            if (finishedAuctions.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Found {Count} finished auctions", finishedAuctions.Count);

            foreach (var auction in finishedAuctions)
            {
                var auctionFinishedEvent = new AuctionFinishedEvent
                {
                    AuctionId = auction.Id,
                    ItemSold = auction.CurrentHighBid != null && auction.CurrentHighBid >= auction.ReversePrice,
                    Winner = auction.Winner,
                    Seller = auction.Seller,
                    SoldAmount = auction.CurrentHighBid
                };

                await eventPublisher.PublishAsync(auctionFinishedEvent, cancellationToken);

                _logger.LogInformation(
                    "Published AuctionFinishedEvent for auction {AuctionId}, Sold: {ItemSold}", 
                    auction.Id, 
                    auctionFinishedEvent.ItemSold);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking finished auctions");
        }
    }
}
