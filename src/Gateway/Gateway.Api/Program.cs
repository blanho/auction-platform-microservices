using Gateway.Api.Extensions;
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
builder.Services.AddHealthChecks();
builder.Services.AddGatewayCors(builder.Configuration);

var app = builder.Build();

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
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapHealthChecks("/health/live").AllowAnonymous();

await app.RunAsync();
