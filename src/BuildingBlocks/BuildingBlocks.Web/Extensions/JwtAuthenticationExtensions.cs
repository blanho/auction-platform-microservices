using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Web.Extensions;

public static class JwtAuthenticationExtensions
{

    public static AuthenticationBuilder AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        var identityAuthority = configuration["Identity:Authority"];
        var secretKey = configuration["Identity:SecretKey"];
        var audience = configuration["Identity:Audience"] ?? "auctionApp";

        return services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !isDevelopment;
            options.SaveToken = true;

            if (!string.IsNullOrEmpty(identityAuthority))
            {
                options.Authority = identityAuthority;
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = identityAuthority,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = !string.IsNullOrEmpty(secretKey)
                    ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    : throw new InvalidOperationException("Identity:SecretKey must be configured for JWT validation"),

                ClockSkew = TimeSpan.FromMinutes(1),

                NameClaimType = "name",
                RoleClaimType = "role"
            };
        });
    }

    public static AuthenticationBuilder AddJwtAuthenticationWithSignalR(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false,
        string hubPath = "/hubs")
    {
        var identityAuthority = configuration["Identity:Authority"];
        var secretKey = configuration["Identity:SecretKey"];
        var audience = configuration["Identity:Audience"] ?? "auctionApp";

        return services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !isDevelopment;
            options.SaveToken = true;

            if (!string.IsNullOrEmpty(identityAuthority))
            {
                options.Authority = identityAuthority;
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = identityAuthority,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = !string.IsNullOrEmpty(secretKey)
                    ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    : throw new InvalidOperationException("Identity:SecretKey must be configured for JWT validation"),

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
                        context.HttpContext.Request.Path.StartsWithSegments(hubPath))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
    }
}
