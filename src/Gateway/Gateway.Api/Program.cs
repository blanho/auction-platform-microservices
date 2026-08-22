using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Globalization;
using Gateway.Api.Extensions;
using Gateway.Api.Constants;
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
            var correlationId = context.HttpContext.Request.Headers[GatewayConstants.CorrelationIdHeader].FirstOrDefault()
                                ?? Guid.NewGuid().ToString();
            context.ProxyRequest.Headers.Remove(GatewayConstants.CorrelationIdHeader);
            context.ProxyRequest.Headers.Add(GatewayConstants.CorrelationIdHeader, correlationId);
            return ValueTask.CompletedTask;
        });
    });

builder.Services.AddGatewayRateLimiter();
builder.Services.AddGatewayAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("GatewayService is running"),
        tags: new[] { "self", "ready" });
builder.Services.AddGatewayCors(builder.Configuration, builder.Environment);

var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ja-JP") };
builder.Services.AddLocalization();
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

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapReverseProxy();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self")
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();

await app.RunAsync();
