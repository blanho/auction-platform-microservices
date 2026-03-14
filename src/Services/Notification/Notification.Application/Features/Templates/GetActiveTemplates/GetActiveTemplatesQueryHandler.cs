using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.GetActiveTemplates;

public class GetActiveTemplatesQueryHandler : IQueryHandler<GetActiveTemplatesQuery, List<TemplateDto>>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<GetActiveTemplatesQueryHandler> _logger;

    public GetActiveTemplatesQueryHandler(
        ITemplateService templateService,
        ILogger<GetActiveTemplatesQueryHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<List<TemplateDto>>> Handle(GetActiveTemplatesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting active templates");

        var result = await _templateService.GetAllActiveAsync(cancellationToken);

        return result;
    }
}
