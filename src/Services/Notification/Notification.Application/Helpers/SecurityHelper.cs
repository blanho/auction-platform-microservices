namespace Notification.Application.Helpers;

public static class SecurityHelper
{
    public static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length < 10)
            return "***";
        return token[..5] + "..." + token[^5..];
    }
}
