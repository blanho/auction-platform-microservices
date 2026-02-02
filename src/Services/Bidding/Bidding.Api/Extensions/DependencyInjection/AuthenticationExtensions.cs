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
        var identityAuthority = configuration["Identity:Authority"];
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = identityAuthority;
            options.RequireHttpsMetadata = !environment.IsDevelopment();
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
            var auctionGrpcUrl = configuration["GrpcServices:AuctionService"] ?? "https://localhost:7001";
            options.Address = new Uri(auctionGrpcUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            if (environment.IsDevelopment())
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            return handler;
        });

        services.AddScoped<IAuctionGrpcClient, AuctionGrpcClient>();

        return services;
    }
}
