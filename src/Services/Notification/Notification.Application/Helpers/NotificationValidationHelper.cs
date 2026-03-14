namespace Notification.Application.Helpers;

public static class NotificationValidationHelper
{
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex >= email.Length - 1)
            return false;

        var dotIndex = email.LastIndexOf('.');
        return dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
    }

    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
    }

    public static bool IsValidUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return Guid.TryParse(userId, out _) || userId.Length >= 3;
    }

    public static bool IsValidTemplateKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return key.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.');
    }
}
