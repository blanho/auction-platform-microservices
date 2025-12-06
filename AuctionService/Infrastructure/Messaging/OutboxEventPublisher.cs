using Common.Messaging.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Infrastructure.Messaging;
public class OutboxEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OutboxEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await _publishEndpoint.Publish(message, cancellationToken);
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default) where T : class
    {
        await _publishEndpoint.PublishBatch(messages, cancellationToken);
    }
}
