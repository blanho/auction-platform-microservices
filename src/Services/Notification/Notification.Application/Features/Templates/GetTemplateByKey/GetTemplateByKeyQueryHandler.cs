using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.GetTemplateByKey;

public class GetTemplateByKeyQueryHandler : IQueryHandler<GetTemplateByKeyQuery, TemplateDto>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<GetTemplateByKeyQueryHandler> _logger;

    public GetTemplateByKeyQueryHandler(
        ITemplateService templateService,
        ILogger<GetTemplateByKeyQueryHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<TemplateDto>> Handle(GetTemplateByKeyQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting template by key: {Key}", request.Key);

        var result = await _templateService.GetByKeyAsync(request.Key, cancellationToken);

        return result;
    }
}
