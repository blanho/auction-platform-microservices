using System.Text;
using System.Text.Encodings.Web;

namespace Identity.Application.Helpers;

public static class TwoFactorHelper
{
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
        var issuer = urlEncoder.Encode(issuerName);
        return new UriBuilder
        {
            Scheme = "otpauth",
            Host = "totp",
            Path = $"{issuer}:{urlEncoder.Encode(email)}",
            Query = $"secret={urlEncoder.Encode(unformattedKey)}&issuer={issuer}&digits=6"
        }.Uri.AbsoluteUri;
    }
}
