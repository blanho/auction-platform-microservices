using Auctions.Application.Features.Auctions.BulkImport;
using Auctions.Application.Features.Auctions.Export;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Application.BackgroundJobs;
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
            services.AddScoped<IAuctionRepository>(sp =>
            {
                var inner = sp.GetRequiredService<AuctionRepository>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<CachedAuctionRepository>>();
                return new CachedAuctionRepository(inner, cache, logger);
            });

            services.AddScoped<AuctionViewRepository>();
            services.AddScoped<IAuctionViewRepository>(sp =>
            {
                var inner = sp.GetRequiredService<AuctionViewRepository>();
                return new CachedAuctionViewRepository(inner);
            });

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBookmarkRepository, BookmarkRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddBulkImport();
            services.AddBackgroundJobProcessing();

            return services;
        }

        public static IServiceCollection AddBackgroundJobProcessing(this IServiceCollection services)
        {
            services.AddBackgroundJobs()
                .AddHandler<ExportAuctionsJobHandler>()
                .Build();

            return services;
        }

        public static IServiceCollection AddBulkImport(this IServiceCollection services)
        {
            services.AddSingleton<IBulkImportJobStore, InMemoryBulkImportJobStore>();
            services.AddSingleton(System.Threading.Channels.Channel.CreateUnbounded<BulkImportJob>());
            services.AddSingleton<IBulkImportService, BulkImportService>();
            services.AddHostedService<BulkImportBackgroundService>();

            return services;
        }
    }
}

