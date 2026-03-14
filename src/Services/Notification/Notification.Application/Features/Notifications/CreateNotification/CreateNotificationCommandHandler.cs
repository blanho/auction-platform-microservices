using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using Notification.Application.DTOs;
using Notification.Application.DTOs.Audit;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Application.Features.Notifications.CreateNotification;

public class CreateNotificationCommandHandler : ICommandHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<CreateNotificationCommandHandler> _logger;
    private readonly IAuditPublisher _auditPublisher;

    public CreateNotificationCommandHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationHubService hubService,
        ILogger<CreateNotificationCommandHandler> logger,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hubService = hubService;
        _logger = logger;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating notification of type {Type}", request.Type);

        if (!Enum.TryParse<NotificationType>(request.Type, true, out var notificationType))
        {
            notificationType = NotificationType.General;
        }

        var notification = NotificationEntity.Create(
            request.UserId,
            request.UserId,
            notificationType,
            request.Title,
            request.Message,
            ChannelType.InApp,
            request.Data,
            null,
            request.AuctionId,
            request.BidId,
            null,
            null);

        notification.MarkAsSent();

        await _repository.CreateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            notification.Id,
            NotificationAuditData.FromNotification(notification),
            AuditAction.Created,
            cancellationToken: cancellationToken);

        var dto = notification.ToDto(_mapper);

        try
        {
            await _hubService.SendNotificationToUserAsync(request.UserId, dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send real-time notification to user {UserId}", request.UserId);
        }

        _logger.LogInformation("Created notification {NotificationId} for user {UserId}", notification.Id, request.UserId);

        return Result.Success(dto);
    }
}
