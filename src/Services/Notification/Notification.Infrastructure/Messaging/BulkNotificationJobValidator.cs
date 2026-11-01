using System.Globalization;
using BuildingBlocks.Application.Localization;
using Notification.Application.DTOs;
using Notification.Application.Localization;
using Notification.Domain.Constants;
using NotificationService.Contracts.Commands;
using NotificationService.Contracts.Enums;

namespace Notification.Infrastructure.Messaging;

internal static class BulkNotificationJobValidator
{
    public static LocalizedMessage? Validate(SendBulkNotificationCommand message)
    {
        if (!message.ParentJobItemId.HasValue || message.ParentJobItemId == Guid.Empty ||
            message.ParentJobId == Guid.Empty || message.RequestedBy == Guid.Empty)
            return new(BulkNotificationMessageKeys.RequesterRequired);
        if (string.IsNullOrWhiteSpace(message.TemplateKey) || string.IsNullOrWhiteSpace(message.Title) ||
            string.IsNullOrWhiteSpace(message.Message) || message.GlobalParameters is null)
            return new(BulkNotificationMessageKeys.ContentRequired);
        if (message.BatchSize < NotificationDefaults.Bulk.MinBatchSize ||
            message.BatchSize > NotificationDefaults.Bulk.MaxBatchSize)
            return new(BulkNotificationMessageKeys.BatchSizeOutOfRange,
                NotificationDefaults.Bulk.MinBatchSize.ToString(CultureInfo.InvariantCulture),
                NotificationDefaults.Bulk.MaxBatchSize.ToString(CultureInfo.InvariantCulture));
        if (message.Recipients is null || message.Recipients.Count == 0 ||
            message.Recipients.Count > NotificationDefaults.Bulk.MaxRecipients ||
            message.Recipients.Any(r => r is null || r.UserId == Guid.Empty || r.Parameters is null))
            return new(BulkNotificationMessageKeys.RecipientsRequired);
        if (message.Recipients.Select(r => r.UserId).Distinct().Count() != message.Recipients.Count)
            return new(BulkNotificationMessageKeys.DuplicateRecipients);
        if (message.Channels is null || message.Channels.Count == 0 || message.Channels.Any(c =>
                c is not (NotificationChannelNames.Email or NotificationChannelNames.Sms or
                    NotificationChannelNames.Push or NotificationChannelNames.InApp)))
            return new(BulkNotificationMessageKeys.ChannelsRequired);
        if (message.Channels.Contains(NotificationChannelNames.Email) &&
            message.Recipients.Any(r => string.IsNullOrWhiteSpace(r.Email)))
            return new(BulkNotificationMessageKeys.EmailRequired);
        if (message.Channels.Contains(NotificationChannelNames.Sms) &&
            message.Recipients.Any(r => string.IsNullOrWhiteSpace(r.PhoneNumber)))
            return new(BulkNotificationMessageKeys.PhoneRequired);
        if (message.ScheduledAt.HasValue)
            return new(BulkNotificationMessageKeys.SchedulingUnsupported);
        return null;
    }
}
