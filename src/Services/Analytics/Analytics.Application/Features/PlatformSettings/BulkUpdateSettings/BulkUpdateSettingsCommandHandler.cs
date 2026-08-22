using MediatR;
using Analytics.Application.Interfaces;
using Analytics.Domain.Entities;
using Analytics.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.PlatformSettings.BulkUpdateSettings;

public class BulkUpdateSettingsCommandHandler : IRequestHandler<BulkUpdateSettingsCommand, Result>
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<BulkUpdateSettingsCommandHandler> _logger;

    public BulkUpdateSettingsCommandHandler(IPlatformSettingRepository settingRepository, ILogger<BulkUpdateSettingsCommandHandler> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(BulkUpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Settings)
        {
            var setting = await _settingRepository.GetByKeyAsync(item.Key, cancellationToken);

            if (setting == null)
            {
                setting = new PlatformSetting
                {
                    Id = Guid.NewGuid(),
                    Key = item.Key,
                    Value = item.Value,
                    Category = SettingCategory.Platform,
                    IsSystem = false,
                    LastModifiedBy = request.ModifiedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await _settingRepository.AddAsync(setting, cancellationToken);
            }
            else
            {
                setting.Value = item.Value;
                setting.LastModifiedBy = request.ModifiedBy;
                setting.UpdatedAt = DateTimeOffset.UtcNow;
                await _settingRepository.UpdateAsync(setting, cancellationToken);
            }
        }

        _logger.LogInformation("Bulk update of {Count} settings by {User}", request.Settings.Count, request.ModifiedBy);

        return Result.Success();
    }
}
