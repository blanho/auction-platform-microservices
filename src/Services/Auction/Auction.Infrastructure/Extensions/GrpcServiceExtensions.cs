using Auctions.Infrastructure.Grpc;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Infrastructure.Extensions;

public static class GrpcServiceExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageGrpcClient, FileStorageGrpcClient>();
        services.AddScoped<IFileConfirmationService, FileConfirmationService>();
        
        return services;
    }
}
