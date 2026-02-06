using Jobs.Application.Interfaces;
using Jobs.Domain.Enums;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging;

public class JobItemDispatcher : IJobItemDispatcher
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<JobItemDispatcher> _logger;

    public JobItemDispatcher(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<JobItemDispatcher> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task DispatchItemsAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            _logger.LogWarning("Cannot dispatch items: Job {JobId} not found", jobId);
            return;
        }

        const int batchSize = 100;
        var totalDispatched = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var pendingItems = await _jobItemRepository.GetPendingItemsByJobIdAsync(
                jobId, batchSize, cancellationToken);

            if (pendingItems.Count == 0)
                break;

            foreach (var item in pendingItems)
            {
                item.MarkProcessing();
                await _jobItemRepository.UpdateAsync(item, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var item in pendingItems)
            {
                await _publishEndpoint.Publish(new ProcessJobItemCommand
                {
                    JobId = job.Id,
                    JobItemId = item.Id,
                    JobType = job.Type.ToString(),
                    PayloadJson = item.PayloadJson,
                    CorrelationId = job.CorrelationId
                }, cancellationToken);
            }

            totalDispatched += pendingItems.Count;

            _logger.LogInformation(
                "Dispatched batch of {Count} items for job {JobId} ({TotalDispatched}/{TotalItems})",
                pendingItems.Count, jobId, totalDispatched, job.TotalItems);
        }

        _logger.LogInformation(
            "Completed dispatching {TotalDispatched} items for job {JobId}",
            totalDispatched, jobId);
    }
}
