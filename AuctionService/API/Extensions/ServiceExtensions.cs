using AuctionService.Application.Interfaces;
using AuctionService.Infrastructure.Data;
using AuctionService.Infrastructure.Repositories;
using AuctionService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Common.Caching.Abstractions;
using Common.Repository.Interfaces;
using Common.Repository.Implementations;
using AutoMapper;
using Serilog;

namespace AuctionService.API.Extensions
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
            services.AddControllers();
            services.AddDbContext<AuctionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            
            services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
            
            services.AddScoped<AuctionRepository>(); 
            services.AddScoped<IAuctionRepository>(sp =>
            {
                var inner = sp.GetRequiredService<AuctionRepository>();
                var cache = sp.GetRequiredService<ICacheService>();
                var mapper = sp.GetRequiredService<IMapper>();
                var logger = sp.GetRequiredService<IAppLogger<CachedAuctionRepository>>();
                return new CachedAuctionRepository(inner, cache, mapper, logger);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFileConfirmationService, FileConfirmationService>();

            return services;
        }
    }
}
