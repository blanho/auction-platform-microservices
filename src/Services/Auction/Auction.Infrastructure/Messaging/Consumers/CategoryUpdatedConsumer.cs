using Catalog.Contracts.Events;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class CategoryUpdatedConsumer : IConsumer<CategoryUpdatedEvent>
{
    private readonly AuctionDbContext _dbContext;
    private readonly ILogger<CategoryUpdatedConsumer> _logger;

    public CategoryUpdatedConsumer(AuctionDbContext dbContext, ILogger<CategoryUpdatedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CategoryUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming CategoryUpdatedIntegrationEvent for CategoryId: {CategoryId}", message.CategoryId);

        var updatedCount = await _dbContext.Set<Item>()
            .Where(i => i.CategoryId == message.CategoryId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.CategoryName, message.Name), context.CancellationToken);

        _logger.LogInformation("Updated CategoryName for {Count} items to '{Name}'", updatedCount, message.Name);
    }
}
