using AuctionService.Contracts.Grpc;
using Bidding.Application.Interfaces;
using Bidding.Infrastructure.Grpc;
using GrpcCore = global::Grpc.Core;
using GrpcConfig = global::Grpc.Net.Client.Configuration;

namespace Bidding.Api.Extensions.DependencyInjection;

public static class GrpcExtensions
{
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
            options.ServiceConfig = new GrpcConfig.ServiceConfig
            {
                MethodConfigs = { new GrpcConfig.MethodConfig
                {
                    Names = { GrpcConfig.MethodName.Default },
                    RetryPolicy = new GrpcConfig.RetryPolicy
                    {
                        MaxAttempts = 3,
                        InitialBackoff = TimeSpan.FromMilliseconds(500),
                        MaxBackoff = TimeSpan.FromSeconds(5),
                        BackoffMultiplier = 2,
                        RetryableStatusCodes = { GrpcCore.StatusCode.Unavailable, GrpcCore.StatusCode.DeadlineExceeded }
                    }
                }}
            };
        });

        services.AddScoped<IAuctionGrpcClient, AuctionGrpcClient>();

        return services;
    }
}
