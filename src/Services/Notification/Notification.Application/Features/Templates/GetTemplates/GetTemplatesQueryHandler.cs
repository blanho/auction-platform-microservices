using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.GetTemplates;

public class GetTemplatesQueryHandler : IQueryHandler<GetTemplatesQuery, PaginatedResult<TemplateDto>>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<GetTemplatesQueryHandler> _logger;

    public GetTemplatesQueryHandler(
        ITemplateService templateService,
        ILogger<GetTemplatesQueryHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<TemplateDto>>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting templates - Page: {Page}", request.Page);

        var result = await _templateService.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        return result;
    }
}
