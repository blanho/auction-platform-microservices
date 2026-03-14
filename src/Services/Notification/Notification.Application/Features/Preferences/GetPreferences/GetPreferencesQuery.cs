using Notification.Application.DTOs;

namespace Notification.Application.Features.Preferences.GetPreferences;

public record GetPreferencesQuery(string UserId) : IQuery<NotificationPreferenceDto>;
