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

        var totalDispatched = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var pendingItems = await _jobItemRepository.GetPendingItemsByJobIdAsync(
                jobId, JobDefaults.Dispatcher.FetchBatchSize, cancellationToken);

            if (pendingItems.Count == 0)
                break;

            foreach (var item in pendingItems)
            {
                item.MarkProcessing();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var commands = pendingItems.Select(item => new ProcessJobItemCommand
            {
                JobId = job.Id,
                JobItemId = item.Id,
                JobType = job.Type.ToString(),
                PayloadJson = item.PayloadJson,
                CorrelationId = job.CorrelationId
            }).ToList();

            foreach (var publishBatch in Chunk(commands, JobDefaults.Dispatcher.PublishBatchSize))
            {
                await Task.WhenAll(publishBatch.Select(cmd =>
                    _publishEndpoint.Publish(cmd, cancellationToken)));
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

    private static IEnumerable<List<T>> Chunk<T>(List<T> source, int chunkSize)
    {
        for (var i = 0; i < source.Count; i += chunkSize)
        {
            yield return source.GetRange(i, Math.Min(chunkSize, source.Count - i));
        }
    }
}
