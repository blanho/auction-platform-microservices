using System.Globalization;
using Gateway.Api.Extensions;
using Microsoft.AspNetCore.Localization;
using Serilog;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{

    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;

    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024;

    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);

    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
});

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(context =>
        {
            const string correlationIdHeader = "X-Correlation-Id";
            var correlationId = context.HttpContext.Request.Headers[correlationIdHeader].FirstOrDefault()
                                ?? Guid.NewGuid().ToString();
            context.ProxyRequest.Headers.Remove(correlationIdHeader);
            context.ProxyRequest.Headers.Add(correlationIdHeader, correlationId);
            return ValueTask.CompletedTask;
        });
    });

builder.Services.AddGatewayRateLimiter();
builder.Services.AddGatewayAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("GatewayService is running"),
        tags: new[] { "self", "ready" });
builder.Services.AddGatewayCors(builder.Configuration);

var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ja-JP") };
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

var app = builder.Build();

app.UseRequestLocalization();
app.UseGatewayExceptionHandler();
app.UseSecurityHeaders();
app.UseCorrelationId();

app.UseSerilogRequestLogging();

app.UseWebSockets();

app.UseCors("AllowAll");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self")
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();

await app.RunAsync();
