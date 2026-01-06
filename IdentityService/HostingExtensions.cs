using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Common.Core.Authorization;
using IdentityService.Data;
using IdentityService.Interfaces;
using IdentityService.Models;
using IdentityService.Repositories;
using IdentityService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
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

        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        builder.Services.AddScoped<IEmailService, EventBasedEmailService>();
        builder.Services.AddScoped<ITokenGenerationService, TokenGenerationService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "identity:";
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddSlidingWindowLimiter("auth", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 2;
                opt.QueueLimit = 0;
            });
            options.AddFixedWindowLimiter("password-reset", opt =>
            {
                opt.PermitLimit = 3;
                opt.Window = TimeSpan.FromMinutes(15);
                opt.QueueLimit = 0;
            });
            options.AddSlidingWindowLimiter("2fa", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.SegmentsPerWindow = 5;
                opt.QueueLimit = 0;
            });
            options.AddFixedWindowLimiter("registration", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromHours(1);
                opt.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? (int)retry.TotalSeconds : 60;
                    
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
                
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests. Please try again later.",
                    retryAfterSeconds = retryAfter
                }, cancellationToken);
            };
        });

        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = builder.Configuration.GetSection("RabbitMq");
                cfg.Host(rabbitConfig["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rabbitConfig["Username"] ?? "guest");
                    h.Password(rabbitConfig["Password"] ?? "guest");
                });
            });
        });

        var identityAuthority = builder.Configuration["Identity:IssuerUri"] ?? "http://localhost:5001";
        var secretKey = builder.Configuration["Identity:SecretKey"]
            ?? throw new InvalidOperationException("Identity:SecretKey is not configured");
        var isLocalDevelopment = builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Local";

        builder.Services.AddAuthentication(options =>
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

        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            builder.Services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                });
        }

        var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
        {
            builder.Services.AddAuthentication()
                .AddFacebook("Facebook", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                    options.Scope.Add("email");
                    options.Scope.Add("public_profile");
                });
        }

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        builder.Services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        builder.Services.AddPermissionBasedAuthorization();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy($"Permission:{Permissions.Users.View}", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Users.View)));
            options.AddPolicy($"Permission:{Permissions.Users.Create}", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Create)));
            options.AddPolicy($"Permission:{Permissions.Users.Edit}", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Edit)));
            options.AddPolicy($"Permission:{Permissions.Users.Delete}", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Delete)));
            options.AddPolicy($"Permission:{Permissions.Users.ManageRoles}", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Users.ManageRoles)));
            options.AddPolicy($"Permission:{Permissions.Users.Ban}", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Ban)));
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        var allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        app.UseCors(policy =>
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
