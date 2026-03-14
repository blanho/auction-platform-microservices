using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.GetTemplateById;

public class GetTemplateByIdQueryHandler : IQueryHandler<GetTemplateByIdQuery, TemplateDto>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<GetTemplateByIdQueryHandler> _logger;

    public GetTemplateByIdQueryHandler(
        ITemplateService templateService,
        ILogger<GetTemplateByIdQueryHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<TemplateDto>> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting template by ID: {TemplateId}", request.Id);

        var result = await _templateService.GetByIdAsync(request.Id, cancellationToken);

        return result;
    }
}
