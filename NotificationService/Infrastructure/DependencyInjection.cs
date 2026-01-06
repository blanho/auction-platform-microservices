using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
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
using Polly;

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

        services.AddHttpClient();
        
        services.Configure<ResendEmailSenderOptions>(configuration.GetSection("Email:Resend"));
        services.AddHttpClient("Resend")
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(500);
                options.Retry.UseJitter = true;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
        services.AddSingleton<INotificationSender, ResendEmailSender>();
        
        services.Configure<TwilioSmsSenderOptions>(configuration.GetSection("Sms:Twilio"));
        services.AddHttpClient("Twilio")
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(500);
                options.Retry.UseJitter = true;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
        services.AddSingleton<INotificationSender, TwilioSmsSender>();
        
        services.Configure<FirebasePushSenderOptions>(configuration.GetSection("Push:Firebase"));
        services.AddHttpClient("Firebase")
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(500);
                options.Retry.UseJitter = true;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
        services.AddSingleton<INotificationSender, FirebasePushSender>();

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
