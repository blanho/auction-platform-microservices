using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Application.BackgroundJobs.Core;
using BuildingBlocks.Application.BackgroundJobs.Queue;
using BuildingBlocks.Application.BackgroundJobs.Storage;

namespace BuildingBlocks.Application.BackgroundJobs.Processing;

public sealed class BackgroundJobProcessor : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IBackgroundJobStore _store;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobProcessor> _logger;
    private readonly BackgroundJobProcessorOptions _options;
    private readonly SemaphoreSlim _workerSemaphore;

    public BackgroundJobProcessor(
        IBackgroundJobQueue queue,
        IBackgroundJobStore store,
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundJobProcessor> logger,
        BackgroundJobProcessorOptions? options = null)
    {
        _queue = queue;
        _store = store;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options ?? new BackgroundJobProcessorOptions();
        _workerSemaphore = new SemaphoreSlim(_options.MaxConcurrentJobs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background job processor started with {WorkerCount} workers", _options.MaxConcurrentJobs);

        var retryTask = ProcessRetryableJobsAsync(stoppingToken);
        var cleanupTask = PeriodicCleanupAsync(stoppingToken);
        var processingTask = ProcessQueueAsync(stoppingToken);

        await Task.WhenAll(retryTask, cleanupTask, processingTask);
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _workerSemaphore.WaitAsync(stoppingToken);
                var job = await _queue.DequeueAsync(stoppingToken);
                _ = ProcessJobAsync(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in job queue processing loop");
                _workerSemaphore.Release();
            }
        }
    }

    private async Task ProcessJobAsync(BackgroundJobState job, CancellationToken stoppingToken)
    {
        using var jobCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        jobCts.CancelAfter(_options.JobTimeout);

        try
        {
            _logger.LogInformation("Processing job {JobId} of type {JobType}", job.Id, job.JobType);

            job.MarkRunning();
            await _store.UpdateAsync(job);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var handler = ResolveHandler(scope.ServiceProvider, job.JobType);

            if (handler is null)
            {
                job.MarkFailed($"No handler found for job type: {job.JobType}");
                await _store.UpdateAsync(job);
                return;
            }

            var context = new BackgroundJobContext(
                job,
                scope.ServiceProvider,
                CreateProgressReporter(job),
                CreateCheckpointSetter(job),
                jobCts.Token);

            await handler.HandleAsync(context);

            job.MarkCompleted();
            await _store.UpdateAsync(job);

            _logger.LogInformation("Job {JobId} completed successfully", job.Id);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            job.MarkCancelled();
            await _store.UpdateAsync(job);
            _logger.LogWarning("Job {JobId} was cancelled due to application shutdown", job.Id);
        }
        catch (OperationCanceledException)
        {
            HandleJobTimeout(job);
            await _store.UpdateAsync(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed with error", job.Id);
            job.MarkFailed(ex.Message);
            await _store.UpdateAsync(job);
            await TryScheduleRetryAsync(job);
        }
        finally
        {
            _workerSemaphore.Release();
        }
    }

    private IBackgroundJobHandler? ResolveHandler(IServiceProvider services, string jobType)
    {
        var handlers = services.GetServices<IBackgroundJobHandler>();
        return handlers.FirstOrDefault(h => h.JobType == jobType);
    }

    private Func<int, string?, Task> CreateProgressReporter(BackgroundJobState job)
    {
        return async (progress, status) =>
        {
            job.UpdateProgress(progress, status);
            await _store.UpdateAsync(job);
        };
    }

    private Func<string, string?, Task> CreateCheckpointSetter(BackgroundJobState job)
    {
        return async (checkpointData, resultFileName) =>
        {
            job.SetCheckpoint(checkpointData);
            if (resultFileName is not null)
                job.SetResultFileName(resultFileName);
            await _store.UpdateAsync(job);
        };
    }

    private void HandleJobTimeout(BackgroundJobState job)
    {
        var timeoutMessage = $"Job timed out after {_options.JobTimeout.TotalMinutes} minutes";
        _logger.LogWarning("Job {JobId} timed out", job.Id);
        job.MarkFailed(timeoutMessage);
    }

    private async Task TryScheduleRetryAsync(BackgroundJobState job)
    {
        if (!job.CanRetry)
        {
            _logger.LogWarning("Job {JobId} has exhausted retry attempts ({RetryCount}/{MaxRetries})",
                job.Id, job.RetryCount, job.MaxRetries);
            return;
        }

        var delay = job.CalculateRetryDelay();
        _logger.LogInformation("Scheduling retry for job {JobId} in {Delay}s (attempt {RetryCount}/{MaxRetries})",
            job.Id, delay.TotalSeconds, job.RetryCount + 1, job.MaxRetries);

        job.IncrementRetry();
        await _store.UpdateAsync(job);
    }

    private async Task ProcessRetryableJobsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_options.RetryCheckInterval, stoppingToken);

                var retryableJobs = await _store.GetRetryableJobsAsync(stoppingToken);

                foreach (var job in retryableJobs)
                {
                    _logger.LogInformation("Re-queuing job {JobId} for retry (attempt {RetryCount})",
                        job.Id, job.RetryCount);

                    await _queue.EnqueueAsync(job, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in retry processing loop");
            }
        }
    }

    private async Task PeriodicCleanupAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_options.CleanupInterval, stoppingToken);
                await _store.CleanupExpiredAsync(stoppingToken);
                _logger.LogDebug("Completed periodic job cleanup");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cleanup loop");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background job processor stopping, waiting for {WorkerCount} workers",
            _options.MaxConcurrentJobs - _workerSemaphore.CurrentCount);

        await base.StopAsync(cancellationToken);

        _workerSemaphore.Dispose();
    }
}

public sealed class BackgroundJobProcessorOptions
{
    public int MaxConcurrentJobs { get; set; } = Environment.ProcessorCount;
    public TimeSpan JobTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan RetryCheckInterval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(15);
}
