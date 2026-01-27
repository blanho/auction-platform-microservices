using Notification.Application.DTOs;

namespace Notification.Application.Features.Templates.GetActiveTemplates;

public record GetActiveTemplatesQuery() : IQuery<List<TemplateDto>>;
