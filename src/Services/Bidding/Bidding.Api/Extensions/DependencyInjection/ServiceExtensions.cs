using Bidding.Application.Interfaces;
using Bidding.Application.Services;
using Bidding.Infrastructure.Extensions;
using Bidding.Infrastructure.Persistence;
using Bidding.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BuildingBlocks.Application.Abstractions.Providers;
using System.Text.Json.Serialization;
using Bidding.Infrastructure.Services;

namespace Bidding.Api.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BidDbContext>(options =>
                options
                    .UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"),
                        npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: BidDefaults.Database.RetryCount,
                                maxRetryDelay: TimeSpan.FromSeconds(BidDefaults.Database.MaxRetryDelaySeconds),
                                errorCodesToAdd: null);
                            npgsqlOptions.CommandTimeout(BidDefaults.Database.CommandTimeoutSeconds);
                        })
                    .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            services.AddAutoMapper(typeof(Bidding.Application.Mappings.MappingProfiles).Assembly);

            services.AddScoped<IBidRepository, BidRepository>();
            services.AddScoped<IAuthoritativeBidReader, AuthoritativeBidReader>();
            services.AddScoped<IAuctionBidFinalizationService, AuctionBidFinalizationService>();
            services.AddScoped<IAutoBidRepository, AutoBidRepository>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
            services.AddScoped<IBidService, BidPlacementService>();
            services.AddScoped<IAuctionBidLock, PostgresAuctionBidLock>();
            services.AddScoped<IAutoBidService, AutoBidService>();
            services.AddScoped<IAuctionSnapshotRepository, CachedAuctionSnapshotRepository>();

            return services;
        }
    }
}
