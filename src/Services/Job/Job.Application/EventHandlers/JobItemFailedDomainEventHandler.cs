using BuildingBlocks.Application.Abstractions.Messaging;
using Jobs.Domain.Events;
using JobService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Jobs.Application.EventHandlers;

public class JobItemFailedDomainEventHandler : INotificationHandler<JobItemFailedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<JobItemFailedDomainEventHandler> _logger;

    public JobItemFailedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<JobItemFailedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(JobItemFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Job item {JobItemId} failed for Job {JobId}, Error: {ErrorMessage}, RetryCount: {RetryCount}",
            notification.JobItemId,
            notification.JobId,
            notification.ErrorMessage,
            notification.RetryCount);

        await _eventPublisher.PublishAsync(new JobProgressUpdatedEvent
        {
            JobId = notification.JobId,
            FailedItems = 1
        }, cancellationToken);
    }
}
