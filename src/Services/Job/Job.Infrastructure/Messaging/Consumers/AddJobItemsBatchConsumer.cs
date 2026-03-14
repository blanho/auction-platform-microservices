using Jobs.Application.Interfaces;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class AddJobItemsBatchConsumer : IConsumer<AddJobItemsBatchCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddJobItemsBatchConsumer> _logger;

    public AddJobItemsBatchConsumer(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddJobItemsBatchConsumer> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AddJobItemsBatchCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received item batch for job {JobId}: {Count} items",
            message.JobId, message.Items.Count);

        var job = await _jobRepository.GetByIdForUpdateAsync(
            message.JobId, context.CancellationToken);

        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found, skipping item batch", message.JobId);
            return;
        }

        var itemPayloads = message.Items
            .Select(i => (i.PayloadJson, i.SequenceNumber));

        await _jobItemRepository.BulkCreateItemsAsync(
            job.Id,
            job.MaxRetryCount,
            itemPayloads,
            context.CancellationToken);

        job.IncrementTotalItems(message.Items.Count);
        await _jobRepository.UpdateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Added {Count} items to job {JobId}, total items now: {TotalItems}",
            message.Items.Count, job.Id, job.TotalItems);
    }
}
