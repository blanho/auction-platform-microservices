using MediatR;
using Analytics.Application.Interfaces;
using Analytics.Application.Errors;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.PlatformSettings.DeleteSetting;

public class DeleteSettingCommandHandler : IRequestHandler<DeleteSettingCommand, Result>
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<DeleteSettingCommandHandler> _logger;

    public DeleteSettingCommandHandler(IPlatformSettingRepository settingRepository, ILogger<DeleteSettingCommandHandler> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _settingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (setting == null)
            return Result.Failure(AnalyticsErrors.Setting.NotFound);

        if (setting.IsSystem)
            return Result.Failure(AnalyticsErrors.Setting.SystemSettingReadOnly);

        await _settingRepository.DeleteAsync(request.Id, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' deleted", setting.Key);

        return Result.Success();
    }
}
