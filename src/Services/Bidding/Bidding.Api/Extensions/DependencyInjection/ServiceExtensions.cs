using Bidding.Application.Interfaces;
using Bidding.Application.Services;
using Bidding.Infrastructure.Extensions;
using Bidding.Infrastructure.Persistence;
using Bidding.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Repository;
using System.Text.Json.Serialization;

namespace Bidding.Api.Extensions.DependencyInjection
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
            services.AddDbContext<BidDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(30);
                    }));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IBidRepository, BidRepository>();
            services.AddScoped<IAutoBidRepository, AutoBidRepository>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
            services.AddScoped<IBidService, BidPlacementService>();
            services.AddScoped<IAutoBidService, AutoBidService>();

            return services;
        }
    }
}
