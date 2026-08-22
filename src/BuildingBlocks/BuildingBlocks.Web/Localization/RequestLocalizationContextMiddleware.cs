using BuildingBlocks.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Localization;

public sealed class RequestLocalizationContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILocalizationService localizer)
    {
        using (RequestLocalizationContext.Push(localizer))
        {
            await next(context);
        }
    }
}
