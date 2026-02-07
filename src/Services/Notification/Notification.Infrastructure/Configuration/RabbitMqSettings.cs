using System.ComponentModel.DataAnnotations;

namespace Notification.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    [Required(ErrorMessage = "RabbitMQ Host is required")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    public ushort Port { get; set; } = 5672;

    public string VirtualHost { get; set; } = "/";

    [Required(ErrorMessage = "RabbitMQ Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "RabbitMQ Password is required")]
    public string Password { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "PrefetchCount must be between 1 and 1000")]
    public int PrefetchCount { get; set; } = 16;

    [Range(1, 1000, ErrorMessage = "ConcurrencyLimit must be between 1 and 1000")]
    public int ConcurrencyLimit { get; set; } = 10;
}
