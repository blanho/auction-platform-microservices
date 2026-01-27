using Notification.Application.DTOs;

namespace Notification.Application.Features.Preferences.UpdatePreferences;

public record UpdatePreferencesCommand(
    string UserId,
    bool EmailEnabled,
    bool PushEnabled,
    bool BidUpdates,
    bool AuctionUpdates,
    bool PromotionalEmails,
    bool SystemAlerts
) : ICommand<NotificationPreferenceDto>;
