using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Identity.Api.Helpers;

public static class HttpContextHelper
{
    public static List<string> GetModelStateErrors(ModelStateDictionary modelState)
    {
        return modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
    }

    public static string? GetIpAddress(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return httpContext.Request.Headers["X-Forwarded-For"]
                .FirstOrDefault()?
                .Split(',')
                .FirstOrDefault()?
                .Trim();
        }
        return httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
