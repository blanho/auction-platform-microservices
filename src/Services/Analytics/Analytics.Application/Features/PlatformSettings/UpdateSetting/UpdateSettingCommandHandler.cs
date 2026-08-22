using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using Analytics.Application.Errors;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.PlatformSettings.UpdateSetting;

public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, Result<PlatformSettingDto>>
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<UpdateSettingCommandHandler> _logger;

    public UpdateSettingCommandHandler(IPlatformSettingRepository settingRepository, ILogger<UpdateSettingCommandHandler> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result<PlatformSettingDto>> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _settingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (setting == null)
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.NotFound);

        setting.Value = request.Dto.Value;
        setting.LastModifiedBy = request.ModifiedBy;
        setting.UpdatedAt = DateTimeOffset.UtcNow;

        await _settingRepository.UpdateAsync(setting, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' updated by {User}", setting.Key, request.ModifiedBy);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }
}
