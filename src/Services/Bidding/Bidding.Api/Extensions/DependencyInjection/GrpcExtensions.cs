using Auctions.Api.Grpc;
using Bidding.Application.Interfaces;
using Bidding.Infrastructure.Grpc;

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
