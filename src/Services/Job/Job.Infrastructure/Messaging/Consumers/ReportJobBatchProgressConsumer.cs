using Jobs.Application.Interfaces;
using Jobs.Domain.Enums;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class ReportJobBatchProgressConsumer : IConsumer<ReportJobBatchProgressCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportJobBatchProgressConsumer> _logger;

    public ReportJobBatchProgressConsumer(
        IJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportJobBatchProgressConsumer> logger)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReportJobBatchProgressCommand> context)
    {
        var message = context.Message;

        var job = await _jobRepository.GetByCorrelationIdAsync(
            message.CorrelationId, context.CancellationToken);

        if (job is null)
        {
            _logger.LogWarning(
                "Job with CorrelationId {CorrelationId} not found, skipping batch progress",
                message.CorrelationId);
            return;
        }

        if (job.Status == JobStatus.Pending)
        {
            job.Start();
        }

        if (job.Status != JobStatus.Processing)
        {
            _logger.LogWarning(
                "Job {JobId} is in {Status} state, cannot report progress",
                job.Id, job.Status);
            return;
        }

        if (message.CompletedCount > 0)
        {
            job.RecordBatchCompleted(message.CompletedCount);
        }

        if (message.FailedCount > 0)
        {
            job.RecordBatchFailed(message.FailedCount);
        }

        await _jobRepository.UpdateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Job {JobId} progress updated: {Completed}/{Total} ({Progress}%)",
            job.Id, job.CompletedItems + job.FailedItems, job.TotalItems, job.ProgressPercentage);
    }
}
