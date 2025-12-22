using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Common.Localization.Extensions;

public static class LocalizationExtensions
{
    public static readonly string[] SupportedCultures = ["en", "ja"];
    public static readonly string DefaultCulture = "en";

    public static IServiceCollection AddCommonLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = SupportedCultures.Select(c => new CultureInfo(c)).ToList();

            options.DefaultRequestCulture = new RequestCulture(DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
        });

        return services;
    }

    public static IApplicationBuilder UseCommonLocalization(this IApplicationBuilder app)
    {
        app.UseRequestLocalization();
        return app;
    }
}
