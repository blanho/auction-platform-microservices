using Auctions.Application.Services;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using ICacheService = BuildingBlocks.Application.Abstractions.ICacheService;
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
        public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentUserName();
            });

            return builder;
        }

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
                options.UseNpgsql(dataSource, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(30);
                }));
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
            services.AddScoped<IAuctionQueryRepository>(sp => sp.GetRequiredService<CachedAuctionRepository>());
            services.AddScoped<IAuctionSchedulerRepository>(sp => sp.GetRequiredService<CachedAuctionRepository>());
            services.AddScoped<IAuctionAnalyticsRepository>(sp => sp.GetRequiredService<CachedAuctionRepository>());
            services.AddScoped<IAuctionUserRepository>(sp => sp.GetRequiredService<CachedAuctionRepository>());
            services.AddScoped<IAuctionExportRepository>(sp => sp.GetRequiredService<CachedAuctionRepository>());

            services.AddScoped<IAuctionViewRepository, AuctionViewRepository>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBookmarkRepository, BookmarkRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();

            services.AddScoped<IPaginatedAuctionQueryService, PaginatedAuctionQueryService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

