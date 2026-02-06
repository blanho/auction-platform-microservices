using BuildingBlocks.Application.Abstractions;
using Jobs.Application.Interfaces;
using Jobs.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jobs.Infrastructure.Workers;

public class JobProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobProcessingWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _stuckJobTimeout = TimeSpan.FromMinutes(30);
    private readonly int _batchSize = 5;

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
                await ProcessPendingJobsAsync(stoppingToken);
                await RecoverStuckJobsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in job processing worker loop");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Job processing worker stopped");
    }

    private async Task ProcessPendingJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IJobItemDispatcher>();

        var pendingJobs = await jobRepository.GetPendingJobsByPriorityAsync(_batchSize, cancellationToken);
        if (pendingJobs.Count == 0)
            return;

        _logger.LogInformation("Found {Count} pending jobs to process", pendingJobs.Count);

        foreach (var job in pendingJobs)
        {
            try
            {
                job.Start();
                await jobRepository.UpdateAsync(job, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                await dispatcher.DispatchItemsAsync(job.Id, cancellationToken);

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
}
