using Jobs.Application.Interfaces;
using Jobs.Domain.Enums;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using Job = Jobs.Domain.Entities.Job;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class RequestJobConsumer : IConsumer<RequestJobCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestJobConsumer> _logger;

    public RequestJobConsumer(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<RequestJobConsumer> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RequestJobCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received job request: Type={JobType}, CorrelationId={CorrelationId}",
            message.JobType, message.CorrelationId);

        var existingJob = await _jobRepository.GetByCorrelationIdAsync(
            message.CorrelationId, context.CancellationToken);

        if (existingJob is not null)
        {
            _logger.LogWarning(
                "Job with CorrelationId {CorrelationId} already exists: {JobId}, skipping",
                message.CorrelationId, existingJob.Id);
            return;
        }

        if (!Enum.TryParse<JobType>(message.JobType, true, out var jobType))
        {
            _logger.LogError("Invalid job type: {JobType}", message.JobType);
            return;
        }

        var priority = Enum.IsDefined(typeof(JobPriority), message.Priority)
            ? (JobPriority)message.Priority
            : JobPriority.Normal;

        var isBulkJob = message.TotalItems > 0 && message.Items.Count == 0;

        var totalItems = isBulkJob
            ? message.TotalItems
            : message.Items.Count > 0
                ? message.Items.Count
                : 1;

        var job = Job.Create(
            jobType,
            message.CorrelationId,
            message.PayloadJson,
            message.RequestedBy,
            totalItems,
            message.MaxRetryCount,
            priority);

        await _jobRepository.CreateAsync(job, context.CancellationToken);

        if (!isBulkJob)
        {
            if (message.Items.Count > 0)
            {
                var items = message.Items
                    .Select(i => job.AddItem(i.PayloadJson, i.SequenceNumber))
                    .ToList();
                await _jobItemRepository.AddRangeAsync(items, context.CancellationToken);
            }
            else
            {
                var singleItem = job.AddItem(message.PayloadJson, 1);
                await _jobItemRepository.AddRangeAsync([singleItem], context.CancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Created job {JobId} of type {JobType} with {TotalItems} items from RequestJobCommand",
            job.Id, job.Type, job.TotalItems);
    }
}
