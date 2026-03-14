using Jobs.Application.Interfaces;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Jobs.Infrastructure.Messaging.Consumers;

public class FinalizeJobInitializationConsumer : IConsumer<FinalizeJobInitializationCommand>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FinalizeJobInitializationConsumer> _logger;

    public FinalizeJobInitializationConsumer(
        IJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        ILogger<FinalizeJobInitializationConsumer> logger)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FinalizeJobInitializationCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received finalization for job {JobId}", message.JobId);

        var job = await _jobRepository.GetByIdForUpdateAsync(
            message.JobId, context.CancellationToken);

        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found, skipping finalization", message.JobId);
            return;
        }

        job.FinalizeInitialization();
        await _jobRepository.UpdateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Finalized job {JobId} with {TotalItems} total items, status is now Pending",
            job.Id, job.TotalItems);
    }
}
