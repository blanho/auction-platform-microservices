using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Models;

namespace Analytics.Api.Interfaces;

public interface IPlatformSettingService
{
    Task<List<PlatformSettingDto>> GetSettingsAsync(SettingCategory? category, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto?> GetSettingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto?> GetSettingByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto> CreateSettingAsync(CreateSettingDto dto, string? modifiedBy, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto> UpdateSettingAsync(Guid id, UpdateSettingDto dto, string? modifiedBy, CancellationToken cancellationToken = default);
    Task DeleteSettingAsync(Guid id, CancellationToken cancellationToken = default);
    Task BulkUpdateSettingsAsync(List<SettingKeyValue> settings, string? modifiedBy, CancellationToken cancellationToken = default);
}
