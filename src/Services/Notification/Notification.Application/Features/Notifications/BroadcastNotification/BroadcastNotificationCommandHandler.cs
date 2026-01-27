using AutoMapper;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Features.Notifications.BroadcastNotification;

public class BroadcastNotificationCommandHandler : ICommandHandler<BroadcastNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<BroadcastNotificationCommandHandler> _logger;

    public BroadcastNotificationCommandHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationHubService hubService,
        ILogger<BroadcastNotificationCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hubService = hubService;
        _logger = logger;
    }

    public async Task<Result<NotificationDto>> Handle(BroadcastNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Broadcasting notification: {Title}", request.Title);

        if (!Enum.TryParse<NotificationType>(request.Type, true, out var notificationType))
        {
            notificationType = NotificationType.General;
        }

        var notification = NotificationEntity.Create(
            "broadcast",
            "broadcast",
            notificationType,
            request.Title,
            request.Message,
            ChannelType.InApp,
            request.TargetRole ?? "all");

        notification.MarkAsSent();

        await _repository.CreateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = notification.ToDto(_mapper);

        try
        {
            await _hubService.SendNotificationToAllAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast real-time notification");
        }

        _logger.LogInformation("Broadcasted notification {NotificationId}", notification.Id);

        return Result.Success(dto);
    }
}
