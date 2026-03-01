using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Commands;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Messaging.Consumers;

public class MarkAllNotificationsReadConsumer : IConsumer<ProcessMarkAllNotificationsReadCommand>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkAllNotificationsReadConsumer> _logger;

    public MarkAllNotificationsReadConsumer(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkAllNotificationsReadConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessMarkAllNotificationsReadCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing mark-all-notifications-read {CorrelationId} for user {UserId}",
            message.CorrelationId, message.UserId);

        await _repository.MarkAllAsReadAsync(message.UserId, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "All notifications marked as read for user {UserId}, CorrelationId: {CorrelationId}",
            message.UserId, message.CorrelationId);
    }
}
