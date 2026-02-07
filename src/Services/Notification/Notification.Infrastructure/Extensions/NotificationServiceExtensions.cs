using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Configuration;
using Notification.Application.Interfaces;
using Notification.Application.Services;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;
using Notification.Infrastructure.Senders;
using SendGrid;
using StackExchange.Redis;
using NotificationUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Extensions;

public static class NotificationServiceExtensions
{

    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services)
    {

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationRecordRepository, NotificationRecordRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();

        services.AddScoped<NotificationUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationServiceImpl>();
        services.AddScoped<INotificationSender, NotificationSender>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<INotificationRecordService, NotificationRecordService>();

        return services;
    }

    public static IServiceCollection AddNotificationSendersDevelopment(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<ISmsSender, SmsNotificationSender>();
        services.AddScoped<IPushSender, PushSender>();

        return services;
    }

    public static IServiceCollection AddNotificationSendersProduction(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddOptions<SendGridOptions>()
            .BindConfiguration(SendGridOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<ISendGridClient>(sp =>
        {
            var options = configuration.GetSection(SendGridOptions.SectionName).Get<SendGridOptions>();
            return new SendGridClient(options?.ApiKey ?? throw new InvalidOperationException("SendGrid API key not configured"));
        });
        services.AddScoped<IEmailSender, SendGridEmailSender>();

        services.AddOptions<FirebaseOptions>()
            .BindConfiguration(FirebaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton(sp =>
        {
            var options = configuration.GetSection(FirebaseOptions.SectionName).Get<FirebaseOptions>();
            if (FirebaseApp.DefaultInstance == null)
            {
                GoogleCredential credential;
                if (!string.IsNullOrEmpty(options?.ServiceAccountJson))
                {
                    credential = GoogleCredential.FromJson(options.ServiceAccountJson);
                }
                else if (!string.IsNullOrEmpty(options?.ServiceAccountPath))
                {
                    credential = GoogleCredential.FromFile(options.ServiceAccountPath);
                }
                else
                {
                    throw new InvalidOperationException("Firebase credentials not configured");
                }

                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = options?.ProjectId
                });
            }
            return FirebaseMessaging.DefaultInstance;
        });
        services.AddScoped<IPushSender, FirebasePushSender>();

        services.AddOptions<TwilioOptions>()
            .BindConfiguration(TwilioOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<ISmsSender, TwilioSmsSender>();

        return services;
    }

    public static IServiceCollection AddNotificationRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddScoped<IIdempotencyService, RedisIdempotencyService>();

        return services;
    }

    public static IServiceCollection AddNotificationMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.ConfigureNotificationMessaging(configuration);
    }
}
