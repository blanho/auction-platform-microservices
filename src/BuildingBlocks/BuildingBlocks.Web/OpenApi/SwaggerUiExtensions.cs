using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.OpenApi;

public static class SwaggerUiExtensions
{
    public static IApplicationBuilder UseCommonSwaggerUI(this IApplicationBuilder app, string? title = null)
    {
        var webApp = (WebApplication)app;

        var provider = webApp.Services.GetService<IApiVersionDescriptionProvider>();

        webApp.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            if (provider != null && provider.ApiVersionDescriptions.Count > 0)
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    var groupName = desc.GroupName;
                    options.SwaggerEndpoint($"../openapi/{groupName}.json", $"{title ?? "API"} {groupName.ToUpperInvariant()}");
                }
            }
            else
            {
                options.SwaggerEndpoint("../openapi/v1.json", title ?? "API v1");
            }
        });

        return app;
    }
}
