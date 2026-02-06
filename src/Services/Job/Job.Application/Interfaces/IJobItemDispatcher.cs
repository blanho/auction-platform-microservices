namespace Jobs.Application.Interfaces;

public interface IJobItemDispatcher
{
    Task DispatchItemsAsync(Guid jobId, CancellationToken cancellationToken = default);
}
