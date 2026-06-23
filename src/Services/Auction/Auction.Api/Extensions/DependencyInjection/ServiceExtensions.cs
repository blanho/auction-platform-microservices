using Auctions.Application.Services;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using AutoMapper;
using Npgsql;
using Serilog;
using System.Text.Json.Serialization;

namespace Auctions.Api.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<AuctionDbContext>(options =>
                options
                    .UseNpgsql(dataSource, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(30);
                    })
                    .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<AuctionRepository>();
            services.AddScoped<CachedAuctionRepository>(sp =>
            {
                var inner = sp.GetRequiredService<AuctionRepository>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<CachedAuctionRepository>>();
                return new CachedAuctionRepository(inner, cache, logger);
            });
            services.AddScoped<IAuctionReadRepository>(sp => sp.GetRequiredService<CachedAuctionRepository>());
            services.AddScoped<IAuctionWriteRepository>(sp => sp.GetRequiredService<CachedAuctionRepository>());

            services.AddScoped<IBookmarkRepository, BookmarkRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IAuctionBulkRepository, AuctionBulkRepository>();
            services.AddScoped<IImportCheckpointRepository, ImportCheckpointRepository>();

            services.AddScoped<IPaginatedAuctionQueryService, PaginatedAuctionQueryService>();

            services.AddGrpcClient<Catalog.Contracts.Grpc.CatalogGrpc.CatalogGrpcClient>((sp, o) =>
            {
                var cfg = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                o.Address = new Uri(cfg["CatalogService:GrpcUrl"] ?? "http://localhost:5013");
            });
            services.AddScoped<ICatalogGrpcClient, CatalogGrpcClient>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

