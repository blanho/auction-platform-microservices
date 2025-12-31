using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Base consumer that provides idempotency and proper exception handling for all notification consumers.
/// </summary>
public abstract class IdempotentConsumerBase<TMessage> : IConsumer<TMessage> where TMessage : class
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);
    
    protected readonly IDistributedCache Cache;
    protected readonly ILogger Logger;
    
    protected IdempotentConsumerBase(IDistributedCache cache, ILogger logger)
    {
        Cache = cache;
        Logger = logger;
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = GetIdempotencyKey(context);
        var consumerName = GetType().Name;
        var cacheKey = $"notification:idempotency:{consumerName}:{messageId}";

        try
        {
            var existing = await Cache.GetStringAsync(cacheKey, context.CancellationToken);
            if (existing != null)
            {
                Logger.LogInformation(
                    "{Consumer}: Message {MessageId} already processed, skipping",
                    consumerName, messageId);
                return;
            }

            await ProcessMessageAsync(context);

            await Cache.SetStringAsync(
                cacheKey,
                DateTime.UtcNow.ToString("O"),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = IdempotencyTtl },
                context.CancellationToken);

            Logger.LogInformation(
                "{Consumer}: Successfully processed message {MessageId}",
                consumerName, messageId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "{Consumer}: Error processing message {MessageId}. Re-throwing for retry.",
                consumerName, messageId);
            throw;
        }
    }

    protected abstract Task ProcessMessageAsync(ConsumeContext<TMessage> context);

    protected virtual string GetIdempotencyKey(ConsumeContext<TMessage> context)
    {
        return context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
    }
}
