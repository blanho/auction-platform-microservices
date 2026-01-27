using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.DeleteTemplate;

public class DeleteTemplateCommandHandler : ICommandHandler<DeleteTemplateCommand>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<DeleteTemplateCommandHandler> _logger;

    public DeleteTemplateCommandHandler(
        ITemplateService templateService,
        ILogger<DeleteTemplateCommandHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting template: {TemplateId}", request.Id);

        var result = await _templateService.DeleteAsync(request.Id, cancellationToken);

        return result;
    }
}
