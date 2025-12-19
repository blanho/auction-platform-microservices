using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Repositories;
using PortInterfaces = NotificationService.Application.Ports;

namespace NotificationService.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<NotificationRepository>();
            services.AddScoped<INotificationRepository>(sp => sp.GetRequiredService<NotificationRepository>());
            services.AddScoped<PortInterfaces.INotificationRepository>(sp => sp.GetRequiredService<NotificationRepository>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<INotificationService, NotificationServiceImpl>();

            services.AddAutoMapper(typeof(Application.Mappings.MappingProfiles));

            return services;
        }
    }
}
