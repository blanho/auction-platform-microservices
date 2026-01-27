using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using Auctions.Infrastructure.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using AutoMapper;
using Npgsql;
using Serilog;
using System.Text.Json.Serialization;

namespace Auctions.Api.Extensions
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
                options.UseNpgsql(dataSource));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            services.AddScoped<AuctionRepository>(); 
            services.AddScoped<IAuctionRepository>(sp =>
            {
                var inner = sp.GetRequiredService<AuctionRepository>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<CachedAuctionRepository>>();
                return new CachedAuctionRepository(inner, cache, logger);
            });

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserAuctionBookmarkRepository, UserAuctionBookmarkRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

