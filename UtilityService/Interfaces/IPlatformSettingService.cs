using UtilityService.Domain.Entities;
using UtilityService.DTOs;

namespace UtilityService.Interfaces;

public interface IPlatformSettingService
{
    Task<List<PlatformSettingDto>> GetSettingsAsync(SettingCategory? category, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto> GetSettingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto> GetSettingByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto> CreateSettingAsync(CreateSettingDto dto, string? modifiedBy, CancellationToken cancellationToken = default);
    Task<PlatformSettingDto> UpdateSettingAsync(Guid id, UpdateSettingDto dto, string? modifiedBy, CancellationToken cancellationToken = default);
    Task DeleteSettingAsync(Guid id, CancellationToken cancellationToken = default);
}
