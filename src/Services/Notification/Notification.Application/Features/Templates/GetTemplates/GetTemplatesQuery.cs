using Notification.Application.DTOs;

namespace Notification.Application.Features.Templates.GetTemplates;

public record GetTemplatesQuery(int Page = 1, int PageSize = 20) : IQuery<PaginatedResult<TemplateDto>>;
