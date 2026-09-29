using Jobs.Application.Interfaces;
using JobService.Contracts.Commands;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class ReportJobItemResultConsumer : IConsumer<ReportJobItemResultCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportJobItemResultConsumer> _logger;

    public ReportJobItemResultConsumer(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportJobItemResultConsumer> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReportJobItemResultCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received result for job item {JobItemId} of job {JobId}: Success={IsSuccess}",
            message.JobItemId, message.JobId, message.IsSuccess);

        var jobItem = await _jobItemRepository.GetByIdForUpdateAsync(
            message.JobItemId, context.CancellationToken);

        if (jobItem is null)
        {
            _logger.LogWarning("Job item {JobItemId} not found, skipping result", message.JobItemId);
            return;
        }

        if (jobItem.JobId != message.JobId)
        {
            _logger.LogWarning(
                "Job item {JobItemId} does not belong to job {JobId}, skipping result",
                message.JobItemId, message.JobId);
            return;
        }

        if ((message.Attempt ?? 0) != jobItem.RetryCount) return;

        if (jobItem.Status != Jobs.Domain.Enums.JobItemStatus.Processing)
        {
            _logger.LogWarning(
                "Job item {JobItemId} is already in terminal state {Status}, skipping result",
                message.JobItemId, jobItem.Status);
            return;
        }

        var job = await _jobRepository.GetByIdForUpdateAsync(
            message.JobId, context.CancellationToken);

        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found, skipping item result", message.JobId);
            return;
        }

        if (job.Status != Jobs.Domain.Enums.JobStatus.Processing) return;

        if (message.IsSuccess)
        {
            jobItem.MarkCompleted();
            job.RecordItemCompleted();

            _logger.LogInformation("Job item {JobItemId} marked as completed", message.JobItemId);
        }
        else
        {
            jobItem.MarkFailed(message.ErrorMessage ?? "Unknown error", message.IsFinalFailure);

            if (jobItem.Status != Jobs.Domain.Enums.JobItemStatus.Processing)
                job.RecordItemFailed();

            _logger.LogWarning(
                "Job item {JobItemId} marked as failed (terminal={IsTerminal}): {Error}",
                message.JobItemId, jobItem.IsTerminal, message.ErrorMessage);
        }

        await _jobItemRepository.UpdateAsync(jobItem, context.CancellationToken);
        await _jobRepository.UpdateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
