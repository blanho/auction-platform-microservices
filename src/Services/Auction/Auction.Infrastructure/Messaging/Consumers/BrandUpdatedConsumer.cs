using Catalog.Contracts.Events;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class BrandUpdatedConsumer : IConsumer<BrandUpdatedEvent>
{
    private readonly AuctionDbContext _dbContext;
    private readonly ILogger<BrandUpdatedConsumer> _logger;

    public BrandUpdatedConsumer(AuctionDbContext dbContext, ILogger<BrandUpdatedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BrandUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming BrandUpdatedIntegrationEvent for BrandId: {BrandId}", message.BrandId);

        var updatedCount = await _dbContext.Set<Item>()
            .Where(i => i.BrandId == message.BrandId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.BrandName, message.Name), context.CancellationToken);

        _logger.LogInformation("Updated BrandName for {Count} items to '{Name}'", updatedCount, message.Name);
    }
}
