using System.Globalization;
using AspNet.Security.OAuth.GitHub;
using Duende.IdentityServer;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace IdentityService;

internal static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, lc) =>
        {
            lc.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                formatProvider: CultureInfo.InvariantCulture);
            lc.Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration);
        });
        return builder;
    }

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configure token lifespan
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("Resend");
        builder.Services.AddScoped<IEmailService, ResendEmailService>();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                
                options.IssuerUri = builder.Configuration["Identity:IssuerUri"] ?? "http://localhost:5001";
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.GetClients(builder.Configuration))
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>()
            .AddLicenseSummary();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        // Configure external authentication providers
        var authBuilder = builder.Services.AddAuthentication();

        // Add JWT Bearer for API endpoints
        var identityAuthority = builder.Configuration["Identity:IssuerUri"] ?? "http://localhost:5001";
        var isLocalDevelopment = builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Local";
        authBuilder.AddJwtBearer("Bearer", options =>
        {
            options.Authority = identityAuthority;
            options.RequireHttpsMetadata = !isLocalDevelopment;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = identityAuthority,
                ValidateAudience = false,
                NameClaimType = "name",
                RoleClaimType = "role"
            };
        });

        // Google OAuth
        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            authBuilder.AddGoogle("Google", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.Scope.Add("email");
                options.Scope.Add("profile");
            });
        }

        // Facebook OAuth
        var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
        {
            authBuilder.AddFacebook("Facebook", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
                options.Scope.Add("email");
                options.Scope.Add("public_profile");
            });
        }

        // GitHub OAuth
        var githubClientId = builder.Configuration["Authentication:GitHub:ClientId"];
        var githubClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        if (!string.IsNullOrEmpty(githubClientId) && !string.IsNullOrEmpty(githubClientSecret))
        {
            authBuilder.AddGitHub("GitHub", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = githubClientId;
                options.ClientSecret = githubClientSecret;
                options.Scope.Add("user:email");
                options.Scope.Add("read:user");
            });
        }

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors(policy =>
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
