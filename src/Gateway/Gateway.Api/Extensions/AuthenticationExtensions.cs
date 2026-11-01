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
        var isLocalDevelopment = IsLocalDevelopment(environment);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                if (string.IsNullOrEmpty(secretKey))
                {
                    options.Authority = identityAuthority;
                }

                options.RequireHttpsMetadata = !isLocalDevelopment;
                options.TokenValidationParameters = CreateTokenValidationParameters(identityAuthority, secretKey);
                options.Events = new JwtBearerEvents { OnMessageReceived = ReadHubAccessTokenAsync };
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
                if (IsLocalDevelopment(environment))
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
    private static bool IsLocalDevelopment(IWebHostEnvironment environment) =>
        environment.IsDevelopment() || environment.EnvironmentName == GatewayConstants.LocalEnvironment;

    private static TokenValidationParameters CreateTokenValidationParameters(string authority, string secretKey)
    {
        var hasSigningKey = !string.IsNullOrEmpty(secretKey);
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = true,
            ValidAudience = GatewayConstants.DefaultAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = hasSigningKey,
            IssuerSigningKey = hasSigningKey ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) : null,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = GatewayConstants.NameClaim,
            RoleClaimType = GatewayConstants.RoleClaim
        };
    }

    private static Task ReadHubAccessTokenAsync(MessageReceivedContext context)
    {
        var accessToken = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(accessToken) && context.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }

}
