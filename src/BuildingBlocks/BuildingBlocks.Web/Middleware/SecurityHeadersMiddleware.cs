using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions? options = null)
    {
        _next = next;
        _options = options ?? new SecurityHeadersOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.Append("X-Content-Type-Options", "nosniff");

        headers.Append("X-Frame-Options", _options.FrameOptions);

        headers.Append("X-XSS-Protection", "1; mode=block");

        headers.Append("Referrer-Policy", _options.ReferrerPolicy);

        headers.Append("Permissions-Policy", _options.PermissionsPolicy);

        if (_options.EnableHsts && !context.Request.IsHttps)
        {
            headers.Append("Strict-Transport-Security",
                $"max-age={_options.HstsMaxAge}; includeSubDomains; preload");
        }

        if (!string.IsNullOrEmpty(_options.ContentSecurityPolicy))
        {
            headers.Append("Content-Security-Policy", _options.ContentSecurityPolicy);
        }

        headers.Remove("X-Powered-By");
        headers.Remove("Server");

        await _next(context);
    }
}

public class SecurityHeadersOptions
{
    public string FrameOptions { get; set; } = "DENY";

    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    public string PermissionsPolicy { get; set; } = "geolocation=(), microphone=(), camera=()";

    public bool EnableHsts { get; set; } = true;

    public int HstsMaxAge { get; set; } = 31536000;

    public string? ContentSecurityPolicy { get; set; }
}

public static class SecurityHeadersExtensions
{

    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app,
        Action<SecurityHeadersOptions>? configure = null)
    {
        var options = new SecurityHeadersOptions();
        configure?.Invoke(options);

        return app.UseMiddleware<SecurityHeadersMiddleware>(options);
    }

    public static IApplicationBuilder UseApiSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseSecurityHeaders(options =>
        {
            options.FrameOptions = "DENY";
            options.EnableHsts = true;
            options.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
        });
    }

    public static IApplicationBuilder UseGatewaySecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseSecurityHeaders(options =>
        {
            options.FrameOptions = "DENY";
            options.EnableHsts = true;
            options.ContentSecurityPolicy = "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self' wss: https:; " +
                "frame-ancestors 'none'";
        });
    }
}
