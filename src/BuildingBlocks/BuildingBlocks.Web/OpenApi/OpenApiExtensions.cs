using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddCommonOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static IApplicationBuilder UseCommonOpenApi(this IApplicationBuilder app)
    {
        var webApp = (WebApplication)app;
        webApp.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });
        webApp.UseSwaggerUI();
        return app;
    }
}
