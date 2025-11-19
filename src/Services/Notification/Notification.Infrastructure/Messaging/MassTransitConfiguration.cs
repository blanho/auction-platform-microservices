using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Consumers;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Messaging;

public static class MassTransitConfiguration
{

    public static IServiceCollection ConfigureNotificationMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddMassTransit(x =>
        {

            x.AddConsumer<NotificationRequestedConsumer>(cfg =>
            {

                cfg.UseMessageRetry(r => r
                    .Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromSeconds(5),
                        maxInterval: TimeSpan.FromMinutes(5),
                        intervalDelta: TimeSpan.FromSeconds(10))
                    .Handle<Exception>(ex =>

                        !IsPermanentError(ex)));

                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddEntityFrameworkOutbox<NotificationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();

                o.QueryDelay = TimeSpan.FromSeconds(1);

                o.QueryTimeout = TimeSpan.FromSeconds(30);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.Port, rabbitMqSettings.VirtualHost, h =>
                {
                    h.Username(rabbitMqSettings.Username);
                    h.Password(rabbitMqSettings.Password);

                    h.Heartbeat(TimeSpan.FromSeconds(30));
                });

                cfg.UseMessageRetry(r => r
                    .Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

                cfg.ReceiveEndpoint("notification-requests", e =>
                {
                    e.ConfigureConsumer<NotificationRequestedConsumer>(context);

                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;

                    e.DiscardFaultedMessages();
                    e.DiscardSkippedMessages();

                    e.UseDelayedRedelivery(r => r
                        .Intervals(
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(30),
                            TimeSpan.FromHours(1)));

                    e.UseInMemoryOutbox(context);
                });

                cfg.UseDelayedMessageScheduler();

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static bool IsPermanentError(Exception ex)
    {

        return ex.Message.Contains("Invalid email") ||
               ex.Message.Contains("Invalid phone") ||
               ex.Message.Contains("Template not found") ||
               ex.Message.Contains("User not found") ||
               ex.Message.Contains("Unregistered");
    }
}

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = string.Empty;
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public int PrefetchCount { get; set; } = 16;

    public int ConcurrencyLimit { get; set; } = 10;
}
