using Microsoft.AspNetCore.Http;

namespace Identity.Api.Helpers;

public static class HttpContextHelper
{
    public static string? GetIpAddress(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
}
