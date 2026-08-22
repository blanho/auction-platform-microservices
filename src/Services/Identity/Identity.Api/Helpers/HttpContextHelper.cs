using Microsoft.AspNetCore.Http;

namespace Identity.Api.Helpers;

public static class HttpContextHelper
{
    public static string? GetIpAddress(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor
                .FirstOrDefault()?
                .Split(',')
                .FirstOrDefault()?
                .Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
