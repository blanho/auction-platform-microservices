using Jobs.Application.Interfaces;
using JobService.Contracts.Commands;

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
            throw new InvalidOperationException($"Job {message.JobId} has not been initialized yet.");
        }

        var existing = await _jobItemRepository.GetItemsByJobIdAsync(job.Id, context.CancellationToken);
        var existingBySequence = existing.ToDictionary(x => x.SequenceNumber);
        var requested = message.Items.GroupBy(x => x.SequenceNumber).Select(group =>
        {
            if (group.Select(x => x.PayloadJson).Distinct().Count() != 1)
                throw new InvalidOperationException("Conflicting payloads for the same item sequence.");
            return group.First();
        }).ToList();
        foreach (var item in requested)
            if (existingBySequence.TryGetValue(item.SequenceNumber, out var previous) && previous.PayloadJson != item.PayloadJson)
                throw new InvalidOperationException("An item sequence cannot be reused with a different payload.");
        var additions = requested.Where(x => !existingBySequence.ContainsKey(x.SequenceNumber)).ToList();
        if (additions.Count == 0) return;
        await _jobItemRepository.AddRangeAsync(additions.Select(x => job.AddItem(x.PayloadJson, x.SequenceNumber)),
            context.CancellationToken);
        job.IncrementTotalItems(additions.Count);
        await _jobRepository.UpdateAsync(job, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Added {Count} items to job {JobId}, total items now: {TotalItems}",
            message.Items.Count, job.Id, job.TotalItems);
    }
}
