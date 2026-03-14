namespace Notification.Application.Helpers;

public static class EmailHelper
{
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        var maskedLocal = localPart.Length > 2
            ? localPart[..2] + new string('*', Math.Min(localPart.Length - 2, 4))
            : localPart;

        return $"{maskedLocal}@{domain}";
    }
}
