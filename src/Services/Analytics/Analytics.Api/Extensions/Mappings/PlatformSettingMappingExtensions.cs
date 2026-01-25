using Analytics.Api.Entities;
using Analytics.Api.Models;

namespace Analytics.Api.Extensions.Mappings;

public static class PlatformSettingMappingExtensions
{
    public static PlatformSettingDto ToDto(this PlatformSetting setting)
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

    public static List<PlatformSettingDto> ToDtoList(this IEnumerable<PlatformSetting> settings)
    {
        return settings.Select(s => s.ToDto()).ToList();
    }
}
