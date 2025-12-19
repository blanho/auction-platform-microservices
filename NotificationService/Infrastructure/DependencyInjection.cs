using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Ports;
using NotificationService.Application.Services;
using NotificationService.Application.UseCases.DismissNotification;
using NotificationService.Application.UseCases.GetNotifications;
using NotificationService.Application.UseCases.MarkAllAsRead;
using NotificationService.Application.UseCases.MarkAsRead;
using NotificationService.Application.UseCases.SendNotification;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Senders;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Templates;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var templatesPath = configuration.GetValue<string>("Templates:Path") 
            ?? Path.Combine(AppContext.BaseDirectory, "Templates");

        services.AddSingleton<ITemplateRepository>(sp =>
            new FileSystemTemplateRepository(
                templatesPath,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<FileSystemTemplateRepository>>()));

        services.AddSingleton<ITemplateRenderer, DefaultTemplateRenderer>();

        services.Configure<SmtpEmailSenderOptions>(configuration.GetSection("Email:Smtp"));
        services.AddSingleton<INotificationSender, SmtpEmailSender>();
        services.AddSingleton<INotificationSender, ConsoleSmsNotificationSender>();
        services.AddSingleton<INotificationSender, ConsolePushNotificationSender>();

        services.AddSingleton<INotificationSenderFactory, NotificationSenderFactory>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "notification:";
        });
        services.AddScoped<IIdempotencyService, RedisIdempotencyService>();
        services.AddScoped<IRealtimeNotificationService, SignalRRealtimeNotificationService>();

        services.AddScoped<INotificationOrchestrator, NotificationOrchestrator>();

        services.AddScoped<ISendNotificationUseCase, SendNotificationUseCase>();
        services.AddScoped<IGetNotificationsUseCase, GetNotificationsUseCase>();
        services.AddScoped<IMarkAsReadUseCase, MarkAsReadUseCase>();
        services.AddScoped<IMarkAllAsReadUseCase, MarkAllAsReadUseCase>();
        services.AddScoped<IDismissNotificationUseCase, DismissNotificationUseCase>();

        return services;
    }
}
