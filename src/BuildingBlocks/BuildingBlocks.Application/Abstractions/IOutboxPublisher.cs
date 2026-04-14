namespace BuildingBlocks.Application.Abstractions;

public interface IOutboxPublisher
{
    Task PublishThroughOutboxAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    Task PublishBatchThroughOutboxAsync<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default) where T : class;
}
