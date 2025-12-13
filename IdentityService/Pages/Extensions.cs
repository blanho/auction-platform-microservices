using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityService.Pages;

public static class Extensions
{
    internal static async Task<bool> GetSchemeSupportsSignOutAsync(this HttpContext context, string scheme)
    {
        var provider = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
        var handler = await provider.GetHandlerAsync(context, scheme);
        return (handler is IAuthenticationSignOutHandler);
    }

    internal static bool IsNativeClient(this AuthorizationRequest context) => !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
               && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);

    internal static IActionResult LoadingPage(this PageModel page, string? redirectUri)
    {
        page.HttpContext.Response.StatusCode = 200;
        page.HttpContext.Response.Headers.Location = "";

        return page.RedirectToPage("/Redirect/Index", new { RedirectUri = redirectUri });
    }

    internal static bool IsRemote(this ConnectionInfo connection)
    {
        var localAddresses = new List<string?> { "127.0.0.1", "::1" };
        if (connection.LocalIpAddress != null)
        {
            localAddresses.Add(connection.LocalIpAddress.ToString());
        }

        if (!localAddresses.Contains(connection.RemoteIpAddress?.ToString()))
        {
            return true;
        }

        return false;
    }
}
