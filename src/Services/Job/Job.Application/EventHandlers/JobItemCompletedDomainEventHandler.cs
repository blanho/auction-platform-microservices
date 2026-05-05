using BuildingBlocks.Application.Abstractions.Messaging;
using Jobs.Domain.Events;
using JobService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Jobs.Application.EventHandlers;

public class JobItemCompletedDomainEventHandler : INotificationHandler<JobItemCompletedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<JobItemCompletedDomainEventHandler> _logger;

    public JobItemCompletedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<JobItemCompletedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(JobItemCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Job item {JobItemId} completed for Job {JobId}, Progress: {CompletedItems}/{TotalItems}",
            notification.JobItemId,
            notification.JobId,
            notification.CompletedItems,
            notification.TotalItems);

        var progressPercentage = notification.TotalItems > 0
            ? Math.Round((decimal)notification.CompletedItems / notification.TotalItems * 100, JobDefaults.Persistence.ProgressDecimalPlaces)
            : 0;

        await _eventPublisher.PublishAsync(new JobProgressUpdatedEvent
        {
            JobId = notification.JobId,
            TotalItems = notification.TotalItems,
            CompletedItems = notification.CompletedItems,
            ProgressPercentage = progressPercentage
        }, cancellationToken);
    }
}
