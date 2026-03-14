using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.UpdateTemplate;

public class UpdateTemplateCommandHandler : ICommandHandler<UpdateTemplateCommand, TemplateDto>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<UpdateTemplateCommandHandler> _logger;

    public UpdateTemplateCommandHandler(
        ITemplateService templateService,
        ILogger<UpdateTemplateCommandHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<TemplateDto>> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating template: {TemplateId}", request.Id);

        var dto = new UpdateTemplateDto
        {
            Name = request.Name,
            Subject = request.Subject,
            Body = request.Body,
            Description = request.Description,
            SmsBody = request.SmsBody,
            PushTitle = request.PushTitle,
            PushBody = request.PushBody,
            IsActive = request.IsActive
        };

        var result = await _templateService.UpdateAsync(request.Id, dto, cancellationToken);

        return result;
    }
}
