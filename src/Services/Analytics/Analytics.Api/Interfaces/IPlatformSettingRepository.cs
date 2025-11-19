using Analytics.Api.Entities;
using Analytics.Api.Enums;

namespace Analytics.Api.Interfaces;

public interface IPlatformSettingRepository
{
    Task<PlatformSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PlatformSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<PlatformSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<PlatformSetting>> GetByCategoryAsync(SettingCategory category, CancellationToken cancellationToken = default);
    Task<PlatformSetting> AddAsync(PlatformSetting setting, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlatformSetting setting, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
