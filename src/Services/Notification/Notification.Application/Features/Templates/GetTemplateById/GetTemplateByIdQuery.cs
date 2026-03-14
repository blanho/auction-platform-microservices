using Notification.Application.DTOs;

namespace Notification.Application.Features.Templates.GetTemplateById;

public record GetTemplateByIdQuery(Guid Id) : IQuery<TemplateDto>;
