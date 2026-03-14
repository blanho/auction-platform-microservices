using BuildingBlocks.Application.Abstractions.Messaging;
using Jobs.Domain.Events;
using JobService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Jobs.Application.EventHandlers;

public class JobCompletedDomainEventHandler : INotificationHandler<JobCompletedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<JobCompletedDomainEventHandler> _logger;

    public JobCompletedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<JobCompletedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(JobCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Job {JobId} completed, Type: {Type}, Completed: {CompletedItems}/{TotalItems}, Failed: {FailedItems}",
            notification.JobId,
            notification.Type,
            notification.CompletedItems,
            notification.TotalItems,
            notification.FailedItems);

        await _eventPublisher.PublishAsync(new JobCompletedEvent
        {
            JobId = notification.JobId,
            Type = (JobService.Contracts.Enums.JobType)(int)notification.Type,
            CorrelationId = notification.CorrelationId,
            TotalItems = notification.TotalItems,
            CompletedItems = notification.CompletedItems,
            FailedItems = notification.FailedItems,
            CompletedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
