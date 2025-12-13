using BidService.Application.Interfaces;
using BidService.Application.Services;
using BidService.Infrastructure.Data;
using BidService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Repository.Interfaces;
using Common.Repository.Implementations;

namespace BidService.API.Extensions
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
            services.AddDbContext<BidDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
            
            services.AddScoped<IBidRepository, BidRepository>();
            services.AddScoped<IAutoBidRepository, AutoBidRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBidService, BidServiceImpl>();
            services.AddScoped<IAutoBidService, AutoBidService>();

            return services;
        }
    }
}
