using Notification.Application.DTOs;

namespace Notification.Application.Features.Templates.UpdateTemplate;

public record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string Subject,
    string Body,
    string? Description = null,
    string? SmsBody = null,
    string? PushTitle = null,
    string? PushBody = null,
    bool IsActive = true
) : ICommand<TemplateDto>;
