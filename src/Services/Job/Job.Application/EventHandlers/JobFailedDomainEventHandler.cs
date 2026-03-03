using BuildingBlocks.Application.Abstractions.Messaging;
using Jobs.Domain.Events;
using JobService.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Jobs.Application.EventHandlers;

public class JobFailedDomainEventHandler : INotificationHandler<JobFailedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<JobFailedDomainEventHandler> _logger;

    public JobFailedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<JobFailedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(JobFailedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogError(
            "Job {JobId} failed, Type: {Type}, Error: {ErrorMessage}",
            notification.JobId,
            notification.Type,
            notification.ErrorMessage);

        await _eventPublisher.PublishAsync(new JobFailedEvent
        {
            JobId = notification.JobId,
            Type = (JobService.Contracts.Enums.JobType)(int)notification.Type,
            CorrelationId = notification.CorrelationId,
            ErrorMessage = notification.ErrorMessage,
            FailedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
