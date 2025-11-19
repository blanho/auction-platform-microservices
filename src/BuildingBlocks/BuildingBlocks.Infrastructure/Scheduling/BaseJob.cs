using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BuildingBlocks.Infrastructure.Scheduling;

public abstract class BaseJob : IJob
{
    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseJob(ILogger logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobName = context.JobDetail.Key.Name;
        var fireTime = context.FireTimeUtc;

        Logger.LogInformation(
            "Job {JobName} starting execution at {FireTime}",
            jobName, fireTime);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var scope = ServiceProvider.CreateScope();
            await ExecuteJobAsync(scope.ServiceProvider, context.CancellationToken);

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
            Logger.LogError(ex,
                "Job {JobName} failed after {ElapsedMs}ms",
                jobName, stopwatch.ElapsedMilliseconds);

            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    protected abstract Task ExecuteJobAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken);
}
