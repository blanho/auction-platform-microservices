using BuildingBlocks.Application.Abstractions;
using Jobs.Application.Interfaces;
using Jobs.Domain.Enums;
using Jobs.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jobs.Infrastructure.Workers;

public class JobProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobProcessingWorker> _logger;
    private readonly TimeSpan _minPollingInterval = TimeSpan.FromSeconds(JobDefaults.Worker.PollingIntervalSeconds);
    private readonly TimeSpan _maxPollingInterval = TimeSpan.FromMinutes(2);
    private readonly TimeSpan _stuckJobTimeout = TimeSpan.FromMinutes(JobDefaults.Worker.StuckJobTimeoutMinutes);
    private readonly int _batchSize = 5;

    private int _consecutiveEmptyPolls;
    private DateTimeOffset _lastSuccessfulPoll = DateTimeOffset.UtcNow;

    public bool IsHealthy => DateTimeOffset.UtcNow - _lastSuccessfulPoll < TimeSpan.FromMinutes(5);

    public JobProcessingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<JobProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job processing worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var jobsProcessed = await ProcessPendingJobsAsync(stoppingToken);
                await RecoverStuckJobsAsync(stoppingToken);

                _lastSuccessfulPoll = DateTimeOffset.UtcNow;

                if (jobsProcessed > 0)
                {
                    _consecutiveEmptyPolls = 0;
                }
                else
                {
                    _consecutiveEmptyPolls++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in job processing worker loop");
                _consecutiveEmptyPolls++;
            }

            var delay = CalculateAdaptiveDelay();
            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("Job processing worker stopped");
    }

    private async Task<int> ProcessPendingJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IJobItemDispatcher>();

        var pendingJobs = await jobRepository.GetPendingJobsByPriorityAsync(_batchSize, cancellationToken);
        if (pendingJobs.Count == 0)
            return 0;

        _logger.LogInformation("Found {Count} pending jobs to process", pendingJobs.Count);

        var processedCount = 0;

        foreach (var job in pendingJobs)
        {
            try
            {
                job.Start();
                await jobRepository.UpdateAsync(job, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                await dispatcher.DispatchItemsAsync(job.Id, cancellationToken);
                processedCount++;

                _logger.LogInformation("Started job {JobId} of type {JobType} with {TotalItems} items",
                    job.Id, job.Type, job.TotalItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start job {JobId}", job.Id);

                try
                {
                    job.Fail(ex.Message);
                    await jobRepository.UpdateAsync(job, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to mark job {JobId} as failed", job.Id);
                }
            }
        }

        return processedCount;
    }

    private async Task RecoverStuckJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var stuckJobs = await jobRepository.GetStuckProcessingJobsAsync(_stuckJobTimeout, cancellationToken);
        if (stuckJobs.Count == 0)
            return;

        _logger.LogWarning("Found {Count} stuck processing jobs, recovering", stuckJobs.Count);

        foreach (var job in stuckJobs)
        {
            try
            {
                job.Fail("Job timed out while processing");
                await jobRepository.UpdateAsync(job, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Recovered stuck job {JobId}", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recover stuck job {JobId}", job.Id);
            }
        }
    }

    private TimeSpan CalculateAdaptiveDelay()
    {
        if (_consecutiveEmptyPolls <= 1)
            return _minPollingInterval;

        var multiplier = Math.Min(_consecutiveEmptyPolls, 6);
        var delay = _minPollingInterval * Math.Pow(1.5, multiplier);

        return delay > _maxPollingInterval ? _maxPollingInterval : delay;
    }
}
