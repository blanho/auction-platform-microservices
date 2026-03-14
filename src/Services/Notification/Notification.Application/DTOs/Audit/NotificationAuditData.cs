using Notification.Domain.Entities;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.DTOs.Audit;

public sealed record NotificationAuditData(
    Guid NotificationId,
    string UserId,
    string Type,
    string Title,
    string Status,
    Guid? AuctionId,
    Guid? BidId,
    Guid? OrderId)
{
    public static NotificationAuditData FromNotification(NotificationEntity notification) => new(
        notification.Id,
        notification.UserId,
        notification.Type.ToString(),
        notification.Title,
        notification.Status.ToString(),
        notification.AuctionId,
        notification.BidId,
        notification.OrderId);
}

public sealed record NotificationPreferenceAuditData(
    Guid PreferenceId,
    string UserId,
    bool EmailEnabled,
    bool PushEnabled,
    bool BidUpdates,
    bool AuctionUpdates,
    bool PromotionalEmails,
    bool SystemAlerts)
{
    public static NotificationPreferenceAuditData FromPreference(NotificationPreference preference) => new(
        preference.Id,
        preference.UserId,
        preference.EmailEnabled,
        preference.PushEnabled,
        preference.BidUpdates,
        preference.AuctionUpdates,
        preference.PromotionalEmails,
        preference.SystemAlerts);
}

public sealed record NotificationTemplateAuditData(
    Guid TemplateId,
    string Key,
    string Name,
    string Subject,
    bool IsActive)
{
    public static NotificationTemplateAuditData FromTemplate(NotificationTemplate template) => new(
        template.Id,
        template.Key,
        template.Name,
        template.Subject,
        template.IsActive);
}
