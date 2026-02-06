using Jobs.Application.Interfaces;
using Jobs.Domain.Enums;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using Job = Jobs.Domain.Entities.Job;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class InitializeStreamingJobConsumer : IConsumer<InitializeStreamingJobCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InitializeStreamingJobConsumer> _logger;

    public InitializeStreamingJobConsumer(
        IJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        ILogger<InitializeStreamingJobConsumer> logger)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InitializeStreamingJobCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received streaming job initialization: Type={JobType}, CorrelationId={CorrelationId}",
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

        var job = Job.CreateStreaming(
            jobType,
            message.CorrelationId,
            message.PayloadJson,
            message.RequestedBy,
            message.MaxRetryCount,
            priority);

        await _jobRepository.CreateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Initialized streaming job {JobId} of type {JobType} with CorrelationId {CorrelationId}",
            job.Id, job.Type, job.CorrelationId);
    }
}
