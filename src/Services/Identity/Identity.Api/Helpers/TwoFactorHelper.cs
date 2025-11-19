using System.Text;
using System.Text.Encodings.Web;

namespace Identity.Api.Helpers;

public static class TwoFactorHelper
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    public static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    public static string GenerateQrCodeUri(UrlEncoder urlEncoder, string email, string unformattedKey, string issuerName = "AuctionPlatform")
    {
        return string.Format(
            AuthenticatorUriFormat,
            urlEncoder.Encode(issuerName),
            urlEncoder.Encode(email),
            unformattedKey);
    }
}
