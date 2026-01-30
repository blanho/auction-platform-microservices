using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Infrastructure.Extensions;

public static class GrpcServiceExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services)
    {
        return services;
    }
}
