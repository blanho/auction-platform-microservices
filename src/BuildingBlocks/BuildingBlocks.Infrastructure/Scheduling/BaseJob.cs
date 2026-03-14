using BuildingBlocks.Application.Abstractions.Locking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BuildingBlocks.Infrastructure.Scheduling;

public abstract class BaseJob : IJob
{
    private const int DefaultMaxRetries = 3;
    private static readonly TimeSpan DefaultBaseDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DefaultLockExpiry = TimeSpan.FromMinutes(10);

    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseJob(ILogger logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    protected virtual int MaxRetries => DefaultMaxRetries;
    protected virtual TimeSpan BaseRetryDelay => DefaultBaseDelay;
    protected virtual bool RequiresDistributedLock => true;
    protected virtual TimeSpan LockExpiry => DefaultLockExpiry;

    public async Task Execute(IJobExecutionContext context)
    {
        var jobName = context.JobDetail.Key.Name;
        var fireTime = context.FireTimeUtc;
        var retryCount = context.RefireCount;

        Logger.LogInformation(
            "Job {JobName} starting execution at {FireTime} (attempt {Attempt}/{MaxAttempts})",
            jobName, fireTime, retryCount + 1, MaxRetries + 1);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var scope = ServiceProvider.CreateScope();

            if (RequiresDistributedLock)
            {
                await ExecuteWithDistributedLockAsync(scope.ServiceProvider, jobName, context.CancellationToken);
            }
            else
            {
                await ExecuteJobAsync(scope.ServiceProvider, context.CancellationToken);
            }

            stopwatch.Stop();
            Logger.LogInformation(
                "Job {JobName} completed successfully in {ElapsedMs}ms",
                jobName, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Job {JobName} was cancelled", jobName);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var shouldRetry = retryCount < MaxRetries && IsTransientException(ex);

            if (shouldRetry)
            {
                var delay = CalculateBackoffDelay(retryCount);
                Logger.LogWarning(ex,
                    "Job {JobName} failed (attempt {Attempt}/{MaxAttempts}), retrying in {DelayMs}ms",
                    jobName, retryCount + 1, MaxRetries + 1, delay.TotalMilliseconds);

                await Task.Delay(delay, context.CancellationToken);
                throw new JobExecutionException(ex, refireImmediately: true);
            }

            Logger.LogError(ex,
                "Job {JobName} failed permanently after {Attempts} attempts in {ElapsedMs}ms",
                jobName, retryCount + 1, stopwatch.ElapsedMilliseconds);

            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    private async Task ExecuteWithDistributedLockAsync(
        IServiceProvider scopedProvider,
        string jobName,
        CancellationToken cancellationToken)
    {
        var distributedLock = scopedProvider.GetService<IDistributedLock>();

        if (distributedLock is null)
        {
            Logger.LogDebug("No IDistributedLock registered, executing {JobName} without lock", jobName);
            await ExecuteJobAsync(scopedProvider, cancellationToken);
            return;
        }

        var lockKey = $"job:{jobName}";
        await using var lockHandle = await distributedLock.TryAcquireAsync(lockKey, LockExpiry, cancellationToken);

        if (lockHandle is null)
        {
            Logger.LogInformation(
                "Job {JobName} skipped — another instance holds the lock", jobName);
            return;
        }

        await ExecuteJobAsync(scopedProvider, cancellationToken);
    }

    protected abstract Task ExecuteJobAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken);

    protected virtual bool IsTransientException(Exception ex) =>
        ex is TimeoutException
            or System.Net.Http.HttpRequestException
            or Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException
            or InvalidOperationException { Message: "An exception has been raised that is likely due to a transient failure." };

    private TimeSpan CalculateBackoffDelay(int retryCount)
    {
        var jitter = Random.Shared.Next(0, 1000);
        var exponentialDelay = BaseRetryDelay * Math.Pow(2, retryCount);
        return exponentialDelay + TimeSpan.FromMilliseconds(jitter);
    }
}
