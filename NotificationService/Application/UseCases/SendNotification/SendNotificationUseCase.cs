using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Ports;
using NotificationService.Application.Services;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Rules;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Application.UseCases.SendNotification;

public interface ISendNotificationUseCase
{
    Task<SendNotificationResponse> ExecuteAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default);
}

public class SendNotificationUseCase : ISendNotificationUseCase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IIdempotencyService _idempotencyService;
    private readonly INotificationOrchestrator _orchestrator;
    private readonly IRealtimeNotificationService _realtimeService;
    private readonly ILogger<SendNotificationUseCase> _logger;

    public SendNotificationUseCase(
        INotificationRepository notificationRepository,
        IIdempotencyService idempotencyService,
        INotificationOrchestrator orchestrator,
        IRealtimeNotificationService realtimeService,
        ILogger<SendNotificationUseCase> logger)
    {
        _notificationRepository = notificationRepository;
        _idempotencyService = idempotencyService;
        _orchestrator = orchestrator;
        _realtimeService = realtimeService;
        _logger = logger;
    }

    public async Task<SendNotificationResponse> ExecuteAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = request.IdempotencyKey ?? GenerateIdempotencyKey(request);

        if (await _idempotencyService.IsProcessedAsync(idempotencyKey, cancellationToken))
        {
            _logger.LogInformation("Notification already processed: {IdempotencyKey}", idempotencyKey);
            var existing = await _notificationRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            return SendNotificationResponse.AlreadyProcessed(existing?.Id ?? Guid.Empty);
        }

        try
        {
            var channels = request.Channels ?? ChannelPolicy.GetDefaultChannels(request.NotificationType);

            var recipient = Recipient.Create(
                request.RecipientId,
                request.RecipientUsername,
                request.RecipientEmail,
                request.RecipientPhone,
                request.DeviceToken);

            channels = ChannelPolicy.FilterByRecipientCapability(channels, recipient);

            var orchestrationRequest = new OrchestrationRequest
            {
                NotificationType = request.NotificationType,
                Recipient = recipient,
                Channels = channels,
                TemplateData = request.TemplateData,
                AuctionId = request.AuctionId,
                OrderId = request.OrderId
            };

            var result = await _orchestrator.OrchestrateAsync(orchestrationRequest, cancellationToken);

            var notification = Notification.Create(
                request.RecipientId,
                request.RecipientUsername,
                request.NotificationType,
                result.Title,
                result.PlainTextBody,
                channels,
                System.Text.Json.JsonSerializer.Serialize(request.TemplateData),
                idempotencyKey,
                request.AuctionId,
                request.BidId,
                request.OrderId,
                request.ReferenceId);

            notification.HtmlContent = result.HtmlBody;

            await _notificationRepository.CreateAsync(notification, cancellationToken);
            notification.MarkAsSent();
            await _notificationRepository.UpdateAsync(notification, cancellationToken);

            await _idempotencyService.MarkAsProcessedAsync(
                idempotencyKey, 
                TimeSpan.FromHours(24), 
                cancellationToken);

            if (channels.HasFlag(ChannelType.InApp))
            {
                var dto = MapToDto(notification);
                await _realtimeService.SendToUserAsync(request.RecipientId, dto);
            }

            _logger.LogInformation(
                "Notification sent: {NotificationId}, Type: {Type}, Channels: {Channels}",
                notification.Id, request.NotificationType, channels);

            return SendNotificationResponse.Succeeded(notification.Id, result.ChannelResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification: {Type} to {Recipient}",
                request.NotificationType, request.RecipientId);
            return SendNotificationResponse.Failed(ex.Message);
        }
    }

    private static string GenerateIdempotencyKey(SendNotificationRequest request)
    {
        var key = IdempotencyKey.Create(
            request.RecipientId,
            request.NotificationType.ToString(),
            request.ReferenceId ?? request.AuctionId?.ToString());
        return key.Value;
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            Data = notification.Data,
            Status = notification.Status.ToString(),
            AuctionId = notification.AuctionId,
            BidId = notification.BidId,
            CreatedAt = notification.CreatedAt
        };
    }
}
