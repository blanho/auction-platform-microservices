using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Identity.Api.DTOs.Audit;
using Identity.Api.DTOs.TwoFactor;
using Identity.Api.Errors;
using Identity.Api.Helpers;
using Identity.Api.Interfaces;
using Identity.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;

namespace Identity.Api.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuditPublisher _auditPublisher;
    private readonly UrlEncoder _urlEncoder;

    public TwoFactorService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAuditPublisher auditPublisher,
        UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _auditPublisher = auditPublisher;
        _urlEncoder = urlEncoder;
    }

    public async Task<Result<TwoFactorStatusResponse>> GetStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<TwoFactorStatusResponse>(IdentityErrors.User.NotFound);

        var isEnabledTask = _userManager.GetTwoFactorEnabledAsync(user);
        var authenticatorKeyTask = _userManager.GetAuthenticatorKeyAsync(user);
        var recoveryCodesTask = _userManager.CountRecoveryCodesAsync(user);
        var isMachineRememberedTask = _signInManager.IsTwoFactorClientRememberedAsync(user);

        await Task.WhenAll(isEnabledTask, authenticatorKeyTask, recoveryCodesTask, isMachineRememberedTask);

        return Result.Success(new TwoFactorStatusResponse
        {
            IsEnabled = isEnabledTask.Result,
            HasAuthenticator = authenticatorKeyTask.Result != null,
            RecoveryCodesLeft = recoveryCodesTask.Result,
            IsMachineRemembered = isMachineRememberedTask.Result
        });
    }

    public async Task<Result<TwoFactorSetupResponse>> SetupAuthenticatorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<TwoFactorSetupResponse>(IdentityErrors.User.NotFound);

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(unformattedKey))
            return Result.Failure<TwoFactorSetupResponse>(IdentityErrors.TwoFactor.SetupFailed);

        var sharedKey = TwoFactorHelper.FormatKey(unformattedKey);
        var authenticatorUri = TwoFactorHelper.GenerateQrCodeUri(_urlEncoder, user.Email!, unformattedKey);

        return Result.Success(new TwoFactorSetupResponse
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeBase64 = string.Empty
        });
    }

    public async Task<Result<RecoveryCodesResponse>> EnableAuthenticatorAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<RecoveryCodesResponse>(IdentityErrors.User.NotFound);

        var verificationCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            verificationCode);

        if (!isValid)
            return Result.Failure<RecoveryCodesResponse>(IdentityErrors.TwoFactor.InvalidCode);

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new TwoFactorAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = "Enable2FA",
                IsEnabled = true
            },
            AuditAction.Updated,
            metadata: new Dictionary<string, object> { ["action"] = "2fa_enabled" });

        return Result.Success(new RecoveryCodesResponse
        {
            RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>()
        });
    }

    public async Task<Result> DisableAuthenticatorAsync(string userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return Result.Failure(IdentityErrors.Auth.InvalidPassword);

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.TwoFactor.DisableFailed);

        await _userManager.ResetAuthenticatorKeyAsync(user);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new TwoFactorAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = "Disable2FA",
                IsEnabled = false
            },
            AuditAction.Updated,
            metadata: new Dictionary<string, object> { ["action"] = "2fa_disabled" });

        return Result.Success();
    }

    public async Task<Result> VerifyCodeAsync(string code, bool rememberDevice)
    {
        var sanitizedCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            sanitizedCode,
            isPersistent: false,
            rememberClient: rememberDevice);

        if (result.Succeeded)
            return Result.Success();

        if (result.IsLockedOut)
            return Result.Failure(IdentityErrors.Auth.AccountLockedOut);

        return Result.Failure(IdentityErrors.TwoFactor.InvalidCode);
    }

    public async Task<Result> UseRecoveryCodeAsync(string recoveryCode)
    {
        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
            return Result.Success();

        if (result.IsLockedOut)
            return Result.Failure(IdentityErrors.Auth.AccountLockedOut);

        return Result.Failure(IdentityErrors.TwoFactor.InvalidRecoveryCode);
    }

    public async Task<Result<RecoveryCodesResponse>> GenerateRecoveryCodesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<RecoveryCodesResponse>(IdentityErrors.User.NotFound);

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return Result.Failure<RecoveryCodesResponse>(IdentityErrors.TwoFactor.NotEnabled);

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return Result.Success(new RecoveryCodesResponse
        {
            RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>()
        });
    }

    public async Task ForgetBrowserAsync()
    {
        await _signInManager.ForgetTwoFactorClientAsync();
    }

    public async Task<Result<TwoFactorStatusResponse>> GetStatusByAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure<TwoFactorStatusResponse>(IdentityErrors.User.NotFound);

        var isEnabledTask = _userManager.GetTwoFactorEnabledAsync(user);
        var authenticatorKeyTask = _userManager.GetAuthenticatorKeyAsync(user);
        var recoveryCodesTask = _userManager.CountRecoveryCodesAsync(user);

        await Task.WhenAll(isEnabledTask, authenticatorKeyTask, recoveryCodesTask);

        return Result.Success(new TwoFactorStatusResponse
        {
            IsEnabled = isEnabledTask.Result,
            HasAuthenticator = authenticatorKeyTask.Result != null,
            RecoveryCodesLeft = recoveryCodesTask.Result,
            IsMachineRemembered = false
        });
    }

    public async Task<Result> ResetByAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        return Result.Success();
    }

    public async Task<Result> DisableByAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return Result.Failure(IdentityErrors.TwoFactor.NotEnabled);

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.TwoFactor.DisableFailed);

        await _userManager.ResetAuthenticatorKeyAsync(user);
        return Result.Success();
    }
}
