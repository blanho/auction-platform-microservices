using BuildingBlocks.Application.Abstractions;

namespace Notification.Application.Errors;

public static class NotificationErrors
{
    public static class Notification
    {
        public static Error NotFound => Error.Create("Notification.NotFound", "Notification not found");
        public static Error NotFoundById(Guid id) => Error.Create("Notification.NotFound", $"Notification with ID {id} was not found");
        public static Error Unauthorized => Error.Create("Notification.Unauthorized", "You are not authorized to access this notification");
        public static Error SendFailed(string reason) => Error.Create("Notification.SendFailed", $"Failed to send notification: {reason}");
        public static Error MarkReadFailed(string reason) => Error.Create("Notification.MarkReadFailed", $"Failed to mark notification as read: {reason}");
    }

    public static class Template
    {
        public static Error NotFound => Error.Create("Template.NotFound", "Notification template not found");
        public static Error NotFoundByKey(string key) => Error.Create("Template.NotFound", $"Notification template '{key}' was not found");
        public static Error KeyExists(string key) => Error.Create("Template.KeyExists", $"Template with key '{key}' already exists");
        public static Error CreateFailed(string reason) => Error.Create("Template.CreateFailed", $"Failed to create template: {reason}");
        public static Error UpdateFailed(string reason) => Error.Create("Template.UpdateFailed", $"Failed to update template: {reason}");
        public static Error DeleteFailed(string reason) => Error.Create("Template.DeleteFailed", $"Failed to delete template: {reason}");
    }

    public static class Preference
    {
        public static Error NotFound => Error.Create("Preference.NotFound", "Notification preference not found");
        public static Error UpdateFailed(string reason) => Error.Create("Preference.UpdateFailed", $"Failed to update preference: {reason}");
    }

    public static class Email
    {
        public static Error SendFailed(string reason) => Error.Create("Email.SendFailed", $"Failed to send email: {reason}");
        public static Error InvalidRecipient => Error.Create("Email.InvalidRecipient", "Invalid email recipient");
    }

    public static class User
    {
        public static Error NotFound => Error.Create("User.NotFound", "User not found");
        public static Error InvalidToken => Error.Create("User.InvalidToken", "User ID not found in token");
    }
}
