using System.Globalization;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Infrastructure.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Web.Extensions;

public static class LocalizationExtensions
{
    public static IServiceCollection AddAppLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions>? configure = null)
    {
        var options = new LocalizationOptions();
        configure?.Invoke(options);

        services.AddSingleton(Options.Create(options));

        services.AddLocalization(localizationOptions =>
        {
            localizationOptions.ResourcesPath = options.ResourcesPath;
        });

        services.AddScoped<ILocalizationService, AppLocalizationService>();

        if (options.EnableRequestLocalization)
        {
            var supportedCultures = options.SupportedCultures
                .Select(c => new CultureInfo(c))
                .ToList();

            services.Configure<RequestLocalizationOptions>(requestOptions =>
            {
                requestOptions.DefaultRequestCulture = new RequestCulture(options.DefaultCulture);
                requestOptions.SupportedCultures = supportedCultures;
                requestOptions.SupportedUICultures = supportedCultures;
                requestOptions.ApplyCurrentCultureToResponseHeaders = true;

                requestOptions.RequestCultureProviders.Clear();

                if (options.UseQueryStringProvider)
                {
                    requestOptions.RequestCultureProviders.Add(
                        new QueryStringRequestCultureProvider
                        {
                            QueryStringKey = options.QueryStringKey,
                            UIQueryStringKey = options.QueryStringKey
                        });
                }

                if (options.UseCookieProvider)
                {
                    requestOptions.RequestCultureProviders.Add(
                        new CookieRequestCultureProvider
                        {
                            CookieName = options.CookieName
                        });
                }

                if (options.UseAcceptLanguageHeader)
                {
                    requestOptions.RequestCultureProviders.Add(
                        new AcceptLanguageHeaderRequestCultureProvider());
                }
            });
        }

        return services;
    }

    public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app)
    {
        var localizationOptions = app.ApplicationServices
            .GetService<IOptions<RequestLocalizationOptions>>();

        if (localizationOptions?.Value != null)
        {
            app.UseRequestLocalization(localizationOptions.Value);
        }

        return app;
    }
}
