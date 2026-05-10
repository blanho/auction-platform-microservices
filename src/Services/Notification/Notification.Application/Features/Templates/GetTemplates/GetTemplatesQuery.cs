using Notification.Application.DTOs;

namespace Notification.Application.Features.Templates.GetTemplates;

public record GetTemplatesQuery(
    int Page = NotificationDefaults.Pagination.DefaultPage,
    int PageSize = NotificationDefaults.Pagination.DefaultPageSize
) : IQuery<PaginatedResult<TemplateDto>>;
