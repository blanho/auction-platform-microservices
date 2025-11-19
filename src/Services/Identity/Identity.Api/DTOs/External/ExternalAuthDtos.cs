using System.ComponentModel.DataAnnotations;

namespace Identity.Api.DTOs.External;

public class ExternalLoginRequest
{
    public string Provider { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = "/";
}

public class ExternalLoginCallbackRequest
{
    public string? RemoteError { get; set; }
    public string ReturnUrl { get; set; } = "/";
}

public class ExchangeCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public class CreateExternalUserRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ProviderKey { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Name { get; set; }
}

public class ExternalLoginTokenResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class ExternalUserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}

public record ExternalAuthResult(string UserId, string AuthCode);
