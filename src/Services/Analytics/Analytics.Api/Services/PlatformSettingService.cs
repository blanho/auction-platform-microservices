using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Web.Exceptions;

namespace Analytics.Api.Services;

public sealed class PlatformSettingService : IPlatformSettingService
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<PlatformSettingService> _logger;

    public PlatformSettingService(
        IPlatformSettingRepository settingRepository,
        ILogger<PlatformSettingService> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<List<PlatformSettingDto>> GetSettingsAsync(
        SettingCategory? category,
        CancellationToken cancellationToken = default)
    {
        var settings = category.HasValue
            ? await _settingRepository.GetByCategoryAsync(category.Value, cancellationToken)
            : await _settingRepository.GetAllAsync(cancellationToken);

        return settings.Select(MapToDto).ToList();
    }

    public async Task<PlatformSettingDto?> GetSettingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByIdAsync(id, cancellationToken);
        return setting is null ? null : MapToDto(setting);
    }

    public async Task<PlatformSettingDto?> GetSettingByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByKeyAsync(key, cancellationToken);
        return setting is null ? null : MapToDto(setting);
    }

    public async Task<PlatformSettingDto> CreateSettingAsync(
        CreateSettingDto dto,
        string? modifiedBy,
        CancellationToken cancellationToken = default)
    {
        if (await _settingRepository.ExistsAsync(dto.Key, cancellationToken))
        {
            throw new ConflictException($"Setting with key '{dto.Key}' already exists.");
        }

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

        return MapToDto(setting);
    }

    public async Task<PlatformSettingDto> UpdateSettingAsync(
        Guid id,
        UpdateSettingDto dto,
        string? modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Setting not found");

        setting.Value = dto.Value;
        setting.LastModifiedBy = modifiedBy;
        setting.UpdatedAt = DateTimeOffset.UtcNow;

        await _settingRepository.UpdateAsync(setting, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' updated by {User}", setting.Key, modifiedBy);

        return MapToDto(setting);
    }

    public async Task DeleteSettingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Setting not found");

        if (setting.IsSystem)
        {
            throw new ConflictException("System settings cannot be deleted.");
        }

        await _settingRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' deleted", setting.Key);
    }

    public async Task BulkUpdateSettingsAsync(
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
    }

    private static PlatformSettingDto MapToDto(PlatformSetting setting)
    {
        return new PlatformSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            Category = setting.Category.ToString(),
            DataType = setting.DataType,
            IsSystem = setting.IsSystem,
            UpdatedAt = setting.UpdatedAt,
            UpdatedBy = setting.LastModifiedBy
        };
    }
}
