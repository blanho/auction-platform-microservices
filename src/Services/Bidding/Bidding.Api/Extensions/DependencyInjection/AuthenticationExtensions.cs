using System.Text;
using Auctions.Api.Grpc;
using Bidding.Application.Interfaces;
using Bidding.Infrastructure.Grpc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Bidding.Api.Extensions.DependencyInjection;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddBiddingAuthentication(
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

    public static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddGrpcClient<AuctionGrpc.AuctionGrpcClient>(options =>
        {
            var auctionGrpcUrl = configuration["GrpcServices:AuctionService"]
                ?? throw new InvalidOperationException("GrpcServices:AuctionService configuration is required");
            options.Address = new Uri(auctionGrpcUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true,
                ConnectTimeout = TimeSpan.FromSeconds(5)
            };
            if (environment.IsDevelopment())
            {
                handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            }
            return handler;
        })
        .ConfigureChannel(options =>
        {
            options.MaxRetryAttempts = 3;
            options.ServiceConfig = new Grpc.Net.Client.Configuration.ServiceConfig
            {
                MethodConfigs = { new Grpc.Net.Client.Configuration.MethodConfig
                {
                    Names = { Grpc.Net.Client.Configuration.MethodName.Default },
                    RetryPolicy = new Grpc.Net.Client.Configuration.RetryPolicy
                    {
                        MaxAttempts = 3,
                        InitialBackoff = TimeSpan.FromMilliseconds(500),
                        MaxBackoff = TimeSpan.FromSeconds(5),
                        BackoffMultiplier = 2,
                        RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable, Grpc.Core.StatusCode.DeadlineExceeded }
                    }
                }}
            };
        });

        services.AddScoped<IAuctionGrpcClient, AuctionGrpcClient>();

        return services;
    }
}
