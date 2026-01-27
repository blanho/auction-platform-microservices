using System.Text;
using BuildingBlocks.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Extensions;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddIdentityAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var identityAuthority = configuration["Identity:IssuerUri"] ?? "http://localhost:5001";
        var secretKey = configuration["Identity:SecretKey"]
            ?? throw new BuildingBlocks.Web.Exceptions.ConfigurationException("Identity:SecretKey is not configured");
        var isLocalDevelopment = environment.IsDevelopment() || environment.EnvironmentName == "Local";

        var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !isLocalDevelopment;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = identityAuthority,
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

        AddExternalProviders(authBuilder, configuration);
        ConfigureCookies(services);
        AddAuthorizationPolicies(services);

        return services;
    }

    private static void AddExternalProviders(AuthenticationBuilder authBuilder, IConfiguration configuration)
    {
        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            authBuilder.AddGoogle("Google", options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.Scope.Add("email");
                options.Scope.Add("profile");
            });
        }

        var facebookAppId = configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
        {
            authBuilder.AddFacebook("Facebook", options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
                options.Scope.Add("email");
                options.Scope.Add("public_profile");
            });
        }
    }

    private static void ConfigureCookies(IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
    }

    private static void AddAuthorizationPolicies(IServiceCollection services)
    {
        services.AddRbacAuthorization();
    }
}
