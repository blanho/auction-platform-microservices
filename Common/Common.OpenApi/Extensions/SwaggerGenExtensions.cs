#nullable enable

using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace Common.OpenApi.Extensions;

public static class SwaggerGenExtensions
{
    public static IServiceCollection AddCommonSwaggerGen(this IServiceCollection services, string title = "API")
    {
        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(sp =>
            new ConfigureSwaggerOptions(sp.GetRequiredService<IApiVersionDescriptionProvider>(), title));
        return services;
    }

    private sealed class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly string _title;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, string title)
        {
            _provider = provider;
            _title = title;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = _title,
                    Version = description.ApiVersion.ToString()
                });
            }
        }

        public void Configure(string? name, SwaggerGenOptions options) => Configure(options);
    }
}
