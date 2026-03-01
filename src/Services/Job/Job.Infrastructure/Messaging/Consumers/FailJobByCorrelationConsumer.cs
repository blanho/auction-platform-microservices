using Jobs.Application.Interfaces;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class FailJobByCorrelationConsumer : IConsumer<FailJobByCorrelationCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FailJobByCorrelationConsumer> _logger;

    public FailJobByCorrelationConsumer(
        IJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        ILogger<FailJobByCorrelationConsumer> logger)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FailJobByCorrelationCommand> context)
    {
        var message = context.Message;

        var job = await _jobRepository.GetByCorrelationIdAsync(
            message.CorrelationId, context.CancellationToken);

        if (job is null)
        {
            _logger.LogWarning(
                "Job with CorrelationId {CorrelationId} not found, skipping fail command",
                message.CorrelationId);
            return;
        }

        if (job.Status is Jobs.Domain.Enums.JobStatus.Completed
            or Jobs.Domain.Enums.JobStatus.CompletedWithErrors
            or Jobs.Domain.Enums.JobStatus.Failed
            or Jobs.Domain.Enums.JobStatus.Cancelled)
        {
            _logger.LogWarning(
                "Job {JobId} is already in terminal state {Status}, skipping fail",
                job.Id, job.Status);
            return;
        }

        job.Fail(message.ErrorMessage);

        await _jobRepository.UpdateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Job {JobId} marked as failed: {ErrorMessage}",
            job.Id, message.ErrorMessage);
    }
}
