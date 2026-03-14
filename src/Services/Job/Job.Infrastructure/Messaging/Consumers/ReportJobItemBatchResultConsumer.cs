using Jobs.Application.Interfaces;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class ReportJobItemBatchResultConsumer : IConsumer<ReportJobItemBatchResultCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportJobItemBatchResultConsumer> _logger;

    public ReportJobItemBatchResultConsumer(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportJobItemBatchResultConsumer> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReportJobItemBatchResultCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received batch result for job {JobId}: {Count} results",
            message.JobId, message.Results.Count);

        var job = await _jobRepository.GetByIdForUpdateAsync(
            message.JobId, context.CancellationToken);

        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found, skipping batch result", message.JobId);
            return;
        }

        var itemIds = message.Results.Select(r => r.JobItemId);
        var jobItems = await _jobItemRepository.GetByIdsForUpdateAsync(
            itemIds, context.CancellationToken);

        var itemLookup = jobItems.ToDictionary(i => i.Id);
        var completedCount = 0;
        var failedCount = 0;

        foreach (var result in message.Results)
        {
            if (!itemLookup.TryGetValue(result.JobItemId, out var jobItem))
            {
                _logger.LogWarning("Job item {JobItemId} not found, skipping", result.JobItemId);
                continue;
            }

            if (jobItem.IsTerminal)
                continue;

            if (result.IsSuccess)
            {
                jobItem.MarkCompleted();
                completedCount++;
            }
            else
            {
                jobItem.MarkFailed(result.ErrorMessage ?? "Unknown error");

                if (jobItem.IsTerminal)
                    failedCount++;
            }

            await _jobItemRepository.UpdateAsync(jobItem, context.CancellationToken);
        }

        if (completedCount > 0)
            job.RecordBatchCompleted(completedCount);

        if (failedCount > 0)
            job.RecordBatchFailed(failedCount);

        await _jobRepository.UpdateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Processed batch result for job {JobId}: {Completed} completed, {Failed} failed",
            job.Id, completedCount, failedCount);
    }
}
