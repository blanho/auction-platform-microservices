using Analytics.Api.Entities;
using Analytics.Api.Enums;

namespace Analytics.Api.Models;

public class PlatformSettingDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? DataType { get; set; }
    public bool IsSystem { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateSettingDto
{
    public string Value { get; set; } = string.Empty;
}

public class CreateSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingCategory Category { get; set; }
    public string? DataType { get; set; }
    public string? ValidationRules { get; set; }
}

public class SettingKeyValue
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
