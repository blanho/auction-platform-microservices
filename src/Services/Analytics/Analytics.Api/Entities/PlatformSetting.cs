using Analytics.Api.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics.Api.Entities;

public class PlatformSetting
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingCategory Category { get; set; }
    public string? DataType { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? ValidationRules { get; set; }
    
    public bool IsSystem { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
