using BuildingBlocks.Application.Abstractions.Messaging;
using Jobs.Domain.Events;
using JobService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Jobs.Application.EventHandlers;

public class JobCreatedDomainEventHandler : INotificationHandler<JobCreatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<JobCreatedDomainEventHandler> _logger;

    public JobCreatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<JobCreatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(JobCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Job {JobId} created, Type: {Type}, TotalItems: {TotalItems}",
            notification.JobId,
            notification.Type,
            notification.TotalItems);

        await _eventPublisher.PublishAsync(new JobCreatedEvent
        {
            JobId = notification.JobId,
            Type = (JobService.Contracts.Enums.JobType)(int)notification.Type,
            CorrelationId = notification.CorrelationId,
            TotalItems = notification.TotalItems,
            RequestedBy = notification.RequestedBy,
            CreatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
