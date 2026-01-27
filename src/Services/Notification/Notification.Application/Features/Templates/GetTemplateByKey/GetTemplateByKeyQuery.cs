using Notification.Application.DTOs;

namespace Notification.Application.Features.Templates.GetTemplateByKey;

public record GetTemplateByKeyQuery(string Key) : IQuery<TemplateDto>;
