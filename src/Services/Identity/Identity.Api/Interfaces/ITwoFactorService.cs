using BuildingBlocks.Application.Abstractions;
using Identity.Api.DTOs.TwoFactor;

namespace Identity.Api.Interfaces;

public interface ITwoFactorService
{
    Task<Result<TwoFactorStatusResponse>> GetStatusAsync(string userId);
    Task<Result<TwoFactorSetupResponse>> SetupAuthenticatorAsync(string userId);
    Task<Result<RecoveryCodesResponse>> EnableAuthenticatorAsync(string userId, string code);
    Task<Result> DisableAuthenticatorAsync(string userId, string password);
    Task<Result> VerifyCodeAsync(string code, bool rememberDevice);
    Task<Result> UseRecoveryCodeAsync(string recoveryCode);
    Task<Result<RecoveryCodesResponse>> GenerateRecoveryCodesAsync(string userId);
    Task ForgetBrowserAsync();
    Task<Result<TwoFactorStatusResponse>> GetStatusByAdminAsync(string userId);
    Task<Result> ResetByAdminAsync(string userId);
    Task<Result> DisableByAdminAsync(string userId);
}
