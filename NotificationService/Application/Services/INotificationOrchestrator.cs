using NotificationService.Application.UseCases.SendNotification;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Application.Services;

public record OrchestrationRequest
{
    public required NotificationType NotificationType { get; init; }
    public required Recipient Recipient { get; init; }
    public required ChannelType Channels { get; init; }
    public Dictionary<string, object> TemplateData { get; init; } = new();
    public Guid? AuctionId { get; init; }
    public Guid? OrderId { get; init; }
}

public record OrchestrationResult
{
    public string Title { get; init; } = string.Empty;
    public string PlainTextBody { get; init; } = string.Empty;
    public string? HtmlBody { get; init; }
    public List<ChannelResult> ChannelResults { get; init; } = new();
}

public interface INotificationOrchestrator
{
    Task<OrchestrationResult> OrchestrateAsync(
        OrchestrationRequest request,
        CancellationToken cancellationToken = default);
}
