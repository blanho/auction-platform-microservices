using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Models;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string JwtId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset? AbsoluteExpiration { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    public bool IsAbsoluteExpired => AbsoluteExpiration.HasValue && DateTimeOffset.UtcNow >= AbsoluteExpiration.Value;

    public bool IsActive => !IsRevoked && !IsExpired && !IsAbsoluteExpired;

    public ApplicationUser User { get; set; } = null!;
}
