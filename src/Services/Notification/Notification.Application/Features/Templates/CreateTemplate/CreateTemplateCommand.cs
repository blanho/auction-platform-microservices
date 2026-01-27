using Notification.Application.DTOs;

namespace Notification.Application.Features.Templates.CreateTemplate;

public record CreateTemplateCommand(
    string Key,
    string Name,
    string Subject,
    string Body,
    string? Description = null,
    string? SmsBody = null,
    string? PushTitle = null,
    string? PushBody = null
) : ICommand<TemplateDto>;
