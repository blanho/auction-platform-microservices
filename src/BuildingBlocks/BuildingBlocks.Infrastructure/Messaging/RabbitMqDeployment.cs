using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Messaging;

public static class RabbitMqDeployment
{
    public static void ConfigureRabbitMqHost(this IRabbitMqBusFactoryConfigurator bus, IConfiguration config)
    {
        var host = config["RabbitMQ:Host"] ?? "localhost";
        var tls = config.GetValue<bool>("RabbitMQ:UseTls");
        var port = config.GetValue<ushort?>("RabbitMQ:Port") ?? (ushort)(tls ? 5671 : 5672);
        bus.Host(host, port, config["RabbitMQ:VirtualHost"] ?? "/", h =>
        {
            h.Username(config["RabbitMQ:Username"] ?? "guest");
            h.Password(config["RabbitMQ:Password"] ?? "guest");
            h.Heartbeat(TimeSpan.FromSeconds(60));
            h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
            h.ContinuationTimeout(TimeSpan.FromSeconds(20));
            if (tls)
                h.UseSsl(ssl =>
                {
                    ssl.Protocol = SslProtocols.Tls12;
                    ssl.ServerName = host;
                });
        });
    }

    public static void ConfigureQuorumQueue(IReceiveEndpointConfigurator endpoint, IRegistrationContext context)
    {
        if (context.GetRequiredService<IConfiguration>().GetValue<bool>("RabbitMQ:UseQuorumQueues")
            && endpoint is IRabbitMqReceiveEndpointConfigurator rabbit)
            rabbit.SetQuorumQueue(3);
    }

    public static string HealthCheckUri(IConfiguration config)
    {
        var tls = config.GetValue<bool>("RabbitMQ:UseTls");
        var port = config.GetValue<ushort?>("RabbitMQ:Port") ?? (ushort)(tls ? 5671 : 5672);
        var host = config["RabbitMQ:Host"] ?? "localhost";
        var user = Uri.EscapeDataString(config["RabbitMQ:Username"] ?? "guest");
        var password = Uri.EscapeDataString(config["RabbitMQ:Password"] ?? "guest");
        var vhost = Uri.EscapeDataString(config["RabbitMQ:VirtualHost"] ?? "/");
        return $"{(tls ? "amqps" : "amqp")}://{user}:{password}@{host}:{port}/{vhost}";
    }
}
