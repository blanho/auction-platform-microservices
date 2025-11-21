using AuctionService.Application.Interfaces;
using AuctionService.Application.Services;
using AuctionService.Infrastructure.Data;
using AuctionService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using AuctionService.Domain.Entities;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Repository.Interfaces;
using Common.Repository.Implementations;
using AutoMapper;

namespace AuctionService.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddDbContext<AuctionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            // Register generic logger
            services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
            
            services.AddScoped<AuctionRepository>(); // inner
            services.AddScoped<IAuctionRepository>(sp =>
            {
                var inner = sp.GetRequiredService<AuctionRepository>();
                var cache = sp.GetRequiredService<ICacheService>();
                var mapper = sp.GetRequiredService<IMapper>();
                return new CachedAuctionRepository(inner, cache, mapper);
            });

            services.AddScoped<IAuctionService, AuctionServiceImpl>();

            return services;
        }
    }
}
