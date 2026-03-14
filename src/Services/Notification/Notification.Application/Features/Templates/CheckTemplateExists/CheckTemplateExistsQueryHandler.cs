using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.CheckTemplateExists;

public class CheckTemplateExistsQueryHandler : IQueryHandler<CheckTemplateExistsQuery, bool>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<CheckTemplateExistsQueryHandler> _logger;

    public CheckTemplateExistsQueryHandler(
        ITemplateService templateService,
        ILogger<CheckTemplateExistsQueryHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CheckTemplateExistsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking if template exists: {Key}", request.Key);

        var exists = await _templateService.ExistsAsync(request.Key, cancellationToken);

        return Result.Success(exists);
    }
}
