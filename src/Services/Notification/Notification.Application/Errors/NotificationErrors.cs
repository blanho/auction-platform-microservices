using BuildingBlocks.Application.Abstractions;

namespace Notification.Application.Errors;

public static class NotificationErrors
{
    public static class Notification
    {
        public static Error NotFound => Error.Create("Notification.NotFound", "Notification not found");
        public static Error NotFoundById(Guid id) => Error.Create("Notification.NotFound", $"Notification with ID {id} was not found");
        public static Error Unauthorized => Error.Create("Notification.Unauthorized", "You are not authorized to access this notification");
        public static Error SendFailed(string reason) => LocalizableError.Localizable("Notification.SendFailed", $"Failed to send notification: {reason}", reason);
        public static Error MarkReadFailed(string reason) => LocalizableError.Localizable("Notification.MarkReadFailed", $"Failed to mark notification as read: {reason}", reason);
    }

    public static class Template
    {
        public static Error NotFound => Error.Create("Template.NotFound", "Notification template not found");
        public static Error NotFoundByKey(string key) => Error.Create("Template.NotFound", $"Notification template '{key}' was not found");
        public static Error KeyExists(string key) => LocalizableError.Localizable("Template.KeyExists", $"Template with key '{key}' already exists", key);
        public static Error CreateFailed(string reason) => LocalizableError.Localizable("Template.CreateFailed", $"Failed to create template: {reason}", reason);
        public static Error UpdateFailed(string reason) => LocalizableError.Localizable("Template.UpdateFailed", $"Failed to update template: {reason}", reason);
        public static Error DeleteFailed(string reason) => LocalizableError.Localizable("Template.DeleteFailed", $"Failed to delete template: {reason}", reason);
    }

    public static class Preference
    {
        public static Error NotFound => Error.Create("Preference.NotFound", "Notification preference not found");
        public static Error UpdateFailed(string reason) => LocalizableError.Localizable("Preference.UpdateFailed", $"Failed to update preference: {reason}", reason);
    }

    public static class Email
    {
        public static Error SendFailed(string reason) => LocalizableError.Localizable("Email.SendFailed", $"Failed to send email: {reason}", reason);
        public static Error InvalidRecipient => Error.Create("Email.InvalidRecipient", "Invalid email recipient");
    }

    public static class User
    {
        public static Error NotFound => Error.Create("User.NotFound", "User not found");
        public static Error InvalidToken => Error.Create("User.InvalidToken", "User ID not found in token");
    }
}
