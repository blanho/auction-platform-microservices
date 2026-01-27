using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Errors;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Services;

public sealed class PlatformSettingService : IPlatformSettingService
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<PlatformSettingService> _logger;
    private const int MaxSettings = 200;

    public PlatformSettingService(
        IPlatformSettingRepository settingRepository,
        ILogger<PlatformSettingService> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result<List<PlatformSettingDto>>> GetSettingsAsync(
        SettingCategory? category,
        CancellationToken cancellationToken = default)
    {
        List<PlatformSetting> settings;
        if (category.HasValue)
        {
            settings = await _settingRepository.GetByCategoryAsync(category.Value, cancellationToken);
        }
        else
        {
            var result = await _settingRepository.GetPagedAsync(1, MaxSettings, cancellationToken);
            settings = result.Items.ToList();
        }

        return Result<List<PlatformSettingDto>>.Success(settings.ToDtoList());
    }

    public async Task<Result<PlatformSettingDto>> GetSettingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByIdAsync(id, cancellationToken);
        if (setting == null)
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.NotFound);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }

    public async Task<Result<PlatformSettingDto>> GetSettingByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByKeyAsync(key, cancellationToken);
        if (setting == null)
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.NotFound);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }

    public async Task<Result<PlatformSettingDto>> CreateSettingAsync(
        CreateSettingDto dto,
        string? modifiedBy,
        CancellationToken cancellationToken = default)
    {
        if (await _settingRepository.ExistsAsync(dto.Key, cancellationToken))
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.KeyExists);

        var setting = new PlatformSetting
        {
            Id = Guid.NewGuid(),
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description,
            Category = dto.Category,
            DataType = dto.DataType,
            ValidationRules = dto.ValidationRules,
            IsSystem = false,
            LastModifiedBy = modifiedBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _settingRepository.AddAsync(setting, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' created by {User}", dto.Key, modifiedBy);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }

    public async Task<Result<PlatformSettingDto>> UpdateSettingAsync(
        Guid id,
        UpdateSettingDto dto,
        string? modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByIdAsync(id, cancellationToken);
        if (setting == null)
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.NotFound);

        setting.Value = dto.Value;
        setting.LastModifiedBy = modifiedBy;
        setting.UpdatedAt = DateTimeOffset.UtcNow;

        await _settingRepository.UpdateAsync(setting, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' updated by {User}", setting.Key, modifiedBy);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }

    public async Task<Result> DeleteSettingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByIdAsync(id, cancellationToken);
        if (setting == null)
            return Result.Failure(AnalyticsErrors.Setting.NotFound);

        if (setting.IsSystem)
            return Result.Failure(AnalyticsErrors.Setting.SystemSettingReadOnly);

        await _settingRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' deleted", setting.Key);

        return Result.Success();
    }

    public async Task<Result> BulkUpdateSettingsAsync(
        List<SettingKeyValue> settings,
        string? modifiedBy,
        CancellationToken cancellationToken = default)
    {
        foreach (var item in settings)
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
                    LastModifiedBy = modifiedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await _settingRepository.AddAsync(setting, cancellationToken);
            }
            else
            {
                setting.Value = item.Value;
                setting.LastModifiedBy = modifiedBy;
                setting.UpdatedAt = DateTimeOffset.UtcNow;
                await _settingRepository.UpdateAsync(setting, cancellationToken);
            }
        }

        _logger.LogInformation("Bulk update of {Count} settings by {User}", settings.Count, modifiedBy);

        return Result.Success();
    }
}
