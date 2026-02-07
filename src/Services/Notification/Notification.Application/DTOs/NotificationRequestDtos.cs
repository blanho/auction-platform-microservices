using Notification.Domain.Enums;

namespace Notification.Application.DTOs;

public record GetAllNotificationsRequest(
    int? Page,
    int? PageSize,
    string? UserId,
    string? Type,
    string? Status);

public record PaginationRequest(
    int? Page,
    int? PageSize);

public record UpdatePreferencesRequest(
    bool EmailEnabled,
    bool PushEnabled,
    bool BidUpdates,
    bool AuctionUpdates,
    bool PromotionalEmails,
    bool SystemAlerts);

public record CreateNotificationRequest(
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? Data = null,
    Guid? AuctionId = null,
    Guid? BidId = null);

public record BroadcastNotificationRequest(
    NotificationType Type,
    string Title,
    string Message,
    string? TargetRole = null);
