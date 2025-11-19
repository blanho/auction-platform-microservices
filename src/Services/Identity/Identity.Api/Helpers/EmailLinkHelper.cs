using System.Web;

namespace Identity.Api.Helpers;

public static class EmailLinkHelper
{
    public static string GenerateConfirmationLink(string frontendUrl, string userId, string token)
    {
        var encodedToken = HttpUtility.UrlEncode(token);
        return $"{frontendUrl}/auth/confirm-email?userId={userId}&token={encodedToken}";
    }

    public static string GeneratePasswordResetLink(string frontendUrl, string email, string token)
    {
        var encodedToken = HttpUtility.UrlEncode(token);
        var encodedEmail = HttpUtility.UrlEncode(email);
        return $"{frontendUrl}/auth/reset-password?email={encodedEmail}&token={encodedToken}";
    }
}
