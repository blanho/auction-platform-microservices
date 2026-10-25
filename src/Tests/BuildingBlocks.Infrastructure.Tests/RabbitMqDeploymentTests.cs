using BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BuildingBlocks.Infrastructure.Tests;

public class RabbitMqDeploymentTests
{
    [Fact]
    public void HealthCheckUsesTlsPortAndEscapesCredentialsAndVirtualHost()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RabbitMQ:Host"] = "broker.example.com",
            ["RabbitMQ:UseTls"] = "true",
            ["RabbitMQ:Port"] = "5671",
            ["RabbitMQ:Username"] = "user@tenant",
            ["RabbitMQ:Password"] = "p:a/ss?#",
            ["RabbitMQ:VirtualHost"] = "tenant/auction",
        }).Build();
        Assert.Equal("amqps://user%40tenant:p%3Aa%2Fss%3F%23@broker.example.com:5671/tenant%2Fauction",
            RabbitMqDeployment.HealthCheckUri(config));
    }

    [Fact]
    public void DevelopmentUsesAmqpAndDefaultVirtualHost()
    {
        var config = new ConfigurationBuilder().Build();
        Assert.Equal("amqp://guest:guest@localhost:5672/%2F", RabbitMqDeployment.HealthCheckUri(config));
    }
}
