using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application.BackgroundJobs.Core;

public interface IBackgroundJobHandler
{
    string JobType { get; }

    Task HandleAsync(BackgroundJobContext context);
}

public sealed class BackgroundJobContext
{
    public BackgroundJobState Job { get; }
    public CancellationToken CancellationToken { get; }

    private readonly IServiceProvider _serviceProvider;
    private readonly Func<int, string?, Task> _onProgressUpdate;
    private readonly Func<string, string?, Task> _onCheckpoint;

    public BackgroundJobContext(
        BackgroundJobState job,
        IServiceProvider serviceProvider,
        Func<int, string?, Task> onProgressUpdate,
        Func<string, string?, Task> onCheckpoint,
        CancellationToken cancellationToken)
    {
        Job = job;
        _serviceProvider = serviceProvider;
        _onProgressUpdate = onProgressUpdate;
        _onCheckpoint = onCheckpoint;
        CancellationToken = cancellationToken;
    }

    public async Task ReportProgressAsync(int processedItems, int totalItems, string? statusMessage = null)
    {
        var percentage = totalItems > 0 ? (int)((double)processedItems / totalItems * 100) : 0;
        Job.UpdateProgress(processedItems, totalItems);
        await _onProgressUpdate(percentage, statusMessage);
    }

    public async Task ReportProgressAsync(int progressPercentage, string? statusMessage = null)
    {
        Job.UpdateProgress(progressPercentage, statusMessage);
        await _onProgressUpdate(progressPercentage, statusMessage);
    }

    public async Task SetCheckpointAsync(string checkpointData, string? resultFileName = null)
    {
        Job.SetCheckpoint(checkpointData);
        if (resultFileName is not null)
            Job.SetResultFileName(resultFileName);
        await _onCheckpoint(checkpointData, resultFileName);
    }

    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public T? GetOptionalService<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }

    public T? GetMetadata<T>(string key)
    {
        if (Job.Metadata.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;

        return default;
    }
}
