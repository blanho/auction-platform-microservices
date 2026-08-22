using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gateway.Api.Constants;

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

        var secretKey = configuration["Identity:SecretKey"];
        var isLocalDevelopment = environment.IsDevelopment() ||
            environment.EnvironmentName == GatewayConstants.LocalEnvironment;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                if (string.IsNullOrEmpty(secretKey))
                {
                    options.Authority = identityAuthority;
                }

                options.RequireHttpsMetadata = !isLocalDevelopment;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = identityAuthority,
                    ValidateAudience = true,
                    ValidAudience = GatewayConstants.DefaultAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = !string.IsNullOrEmpty(secretKey),
                    IssuerSigningKey = !string.IsNullOrEmpty(secretKey)
                        ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                        : null,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = GatewayConstants.NameClaim,
                    RoleClaimType = GatewayConstants.RoleClaim
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
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            options.AddPolicy("AllowAll", policy =>
            {
                if (environment.IsDevelopment() ||
                    environment.EnvironmentName == GatewayConstants.LocalEnvironment)
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                          .WithHeaders(
                              "Authorization",
                              "Content-Type",
                              "X-Requested-With",
                              "Accept",
                              GatewayConstants.CorrelationIdHeader,
                              "X-SignalR-User-Agent")
                          .AllowCredentials();
                }
            });
        });

        return services;
    }
}
