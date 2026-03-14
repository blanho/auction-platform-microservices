using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Templates.CreateTemplate;

public class CreateTemplateCommandHandler : ICommandHandler<CreateTemplateCommand, TemplateDto>
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<CreateTemplateCommandHandler> _logger;

    public CreateTemplateCommandHandler(
        ITemplateService templateService,
        ILogger<CreateTemplateCommandHandler> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<TemplateDto>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating template with key: {Key}", request.Key);

        var dto = new CreateTemplateDto
        {
            Key = request.Key,
            Name = request.Name,
            Subject = request.Subject,
            Body = request.Body,
            Description = request.Description,
            SmsBody = request.SmsBody,
            PushTitle = request.PushTitle,
            PushBody = request.PushBody
        };

        var result = await _templateService.CreateAsync(dto, cancellationToken);

        return result;
    }
}
