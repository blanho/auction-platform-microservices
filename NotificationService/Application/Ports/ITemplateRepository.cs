using NotificationService.Domain.Enums;
using NotificationService.Domain.Models;

namespace NotificationService.Application.Ports;

public interface ITemplateRepository
{
    Task<Template?> GetTemplateAsync(
        NotificationType notificationType, 
        ChannelType channel, 
        string? version = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Template>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    
    Task<bool> TemplateExistsAsync(
        NotificationType notificationType, 
        ChannelType channel,
        CancellationToken cancellationToken = default);
}
