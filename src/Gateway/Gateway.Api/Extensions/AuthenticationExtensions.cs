using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddGatewayAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var identityAuthority = configuration["Identity:Authority"];
        if (string.IsNullOrEmpty(identityAuthority))
            return services;

        var isLocalDevelopment = environment.IsDevelopment() || environment.EnvironmentName == "Local";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = identityAuthority;
                options.RequireHttpsMetadata = !isLocalDevelopment;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = identityAuthority,
                    ValidateAudience = true,
                    ValidAudience = "auctionApp",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? throw new InvalidOperationException("Cors:AllowedOrigins configuration is required");

            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                      .WithHeaders(
                          "Authorization",
                          "Content-Type",
                          "X-Requested-With",
                          "Accept",
                          "X-Correlation-Id",
                          "X-SignalR-User-Agent")
                      .AllowCredentials();
            });
        });

        return services;
    }
}
