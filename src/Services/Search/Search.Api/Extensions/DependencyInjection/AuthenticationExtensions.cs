using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Search.Api.Extensions.DependencyInjection;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSearchAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var issuer = configuration["Identity:IssuerUri"] ?? configuration["Identity:Authority"] ?? "http://localhost:5001";
        var secretKey = configuration["Identity:SecretKey"]
            ?? throw new InvalidOperationException("Identity:SecretKey configuration is required");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !environment.IsDevelopment();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = "auctionApp",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = "name",
                RoleClaimType = "role"
            };
        });

        return services;
    }
}
