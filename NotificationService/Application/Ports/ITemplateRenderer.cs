using NotificationService.Domain.Enums;
using NotificationService.Domain.Models;

namespace NotificationService.Application.Ports;

public interface ITemplateRenderer
{
    ChannelType SupportedChannel { get; }
    
    Task<RenderedContent> RenderAsync(
        Template template, 
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default);
}

public record RenderedContent(
    string Subject,
    string Body,
    string? HtmlBody = null,
    Dictionary<string, string>? Metadata = null);
