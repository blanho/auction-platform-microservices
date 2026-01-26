using Analytics.Api.Enums;
using Analytics.Api.Models;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Interfaces;

public interface IPlatformSettingService
{
    Task<Result<List<PlatformSettingDto>>> GetSettingsAsync(SettingCategory? category, CancellationToken cancellationToken = default);
    Task<Result<PlatformSettingDto>> GetSettingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PlatformSettingDto>> GetSettingByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Result<PlatformSettingDto>> CreateSettingAsync(CreateSettingDto dto, string? modifiedBy, CancellationToken cancellationToken = default);
    Task<Result<PlatformSettingDto>> UpdateSettingAsync(Guid id, UpdateSettingDto dto, string? modifiedBy, CancellationToken cancellationToken = default);
    Task<Result> DeleteSettingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> BulkUpdateSettingsAsync(List<SettingKeyValue> settings, string? modifiedBy, CancellationToken cancellationToken = default);
}
