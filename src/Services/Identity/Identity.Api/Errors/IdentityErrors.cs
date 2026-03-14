using BuildingBlocks.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Errors;

public static class IdentityErrors
{
    public static class Auth
    {
        public static Error UsernameExists => Error.Create("Auth.UsernameExists", "Username already exists");
        public static Error EmailExists => Error.Create("Auth.EmailExists", "Email already registered");
        public static Error InvalidCredentials => Error.Create("Auth.InvalidCredentials", "Invalid username or password");
        public static Error InvalidPassword => Error.Create("Auth.InvalidPassword", "Invalid password");
        public static Error AccountLocked => Error.Create("Auth.AccountLocked", "Account is locked. Please try again later.");
        public static Error AccountLockedOut => Error.Create("Auth.AccountLockedOut", "Account locked out. Please try again later.");
        public static Error AccountSuspended(string? reason) => Error.Create("Auth.AccountSuspended", reason ?? "Account has been suspended");
        public static Error AccountInactive => Error.Create("Auth.AccountInactive", "Account is not active");
        public static Error EmailNotConfirmed => Error.Create("Auth.EmailNotConfirmed", "Email not confirmed. Please check your inbox.");
        public static Error EmailAlreadyConfirmed => Error.Create("Auth.EmailAlreadyConfirmed", "Email is already confirmed");
        public static Error InvalidConfirmationLink => Error.Create("Auth.InvalidConfirmationLink", "Invalid confirmation link");
        public static Error ConfirmationFailed => Error.Create("Auth.ConfirmationFailed", "Email confirmation failed. The link may have expired.");
        public static Error InvalidResetRequest => Error.Create("Auth.InvalidResetRequest", "Invalid reset request");
        public static Error ResetFailed => Error.Create("Auth.ResetFailed", "Password reset failed. The link may have expired.");
        public static Error RegistrationFailed => Error.Create("Auth.RegistrationFailed", "Registration failed");
        public static Error InvalidRefreshToken => Error.Create("Auth.InvalidRefreshToken", "Invalid or expired refresh token");
        public static Error SecurityTermination => Error.Create("Auth.SecurityTermination", "Session terminated due to security concerns");
        public static Error SendEmailFailed => Error.Create("Auth.SendEmailFailed", "Failed to send email. Please try again later.");
        public static Error InvalidAuthCode => Error.Create("Auth.InvalidAuthCode", "Invalid or expired authorization code");
        public static Error AuthCodeExchangeFailed => Error.Create("Auth.AuthCodeExchangeFailed", "Failed to exchange authorization code");
    }

    public static class User
    {
        public static Error NotFound => Error.Create("User.NotFound", "User not found");
        public static Error UpdateFailed => Error.Create("User.UpdateFailed", "Failed to update user");
        public static Error DeleteFailed => Error.Create("User.DeleteFailed", "Failed to delete user");
        public static Error SuspendFailed => Error.Create("User.SuspendFailed", "Failed to suspend user");
        public static Error UnsuspendFailed => Error.Create("User.UnsuspendFailed", "Failed to unsuspend user");
        public static Error ActivateFailed => Error.Create("User.ActivateFailed", "Failed to activate user");
        public static Error DeactivateFailed => Error.Create("User.DeactivateFailed", "Failed to deactivate user");
        public static Error RoleUpdateFailed => Error.Create("User.RoleUpdateFailed", "Failed to update user roles");
        public static Error CannotDeleteSelf => Error.Create("User.CannotDeleteSelf", "Cannot delete your own account");
        public static Error AlreadySeller => Error.Create("User.AlreadySeller", "You are already a seller");
        public static Error AdminHasSellerPrivileges => Error.Create("User.AdminHasSellerPrivileges", "Admin accounts have seller privileges by default");
        public static Error SellerUpgradeFailed => Error.Create("User.SellerUpgradeFailed", "Failed to upgrade to seller");
    }

    public static class TwoFactor
    {
        public static Error NotEnabled => Error.Create("TwoFactor.NotEnabled", "Two-factor authentication is not enabled");
        public static Error AlreadyEnabled => Error.Create("TwoFactor.AlreadyEnabled", "Two-factor authentication is already enabled");
        public static Error InvalidCode => Error.Create("TwoFactor.InvalidCode", "Invalid verification code");
        public static Error InvalidRecoveryCode => Error.Create("TwoFactor.InvalidRecoveryCode", "Invalid recovery code");
        public static Error InvalidPassword => Error.Create("TwoFactor.InvalidPassword", "Invalid password");
        public static Error DisableFailed => Error.Create("TwoFactor.DisableFailed", "Failed to disable two-factor authentication");
        public static Error SetupFailed => Error.Create("TwoFactor.SetupFailed", "Failed to setup two-factor authentication");
        public static Error AccountLocked => Error.Create("TwoFactor.AccountLocked", "Account is locked due to too many failed attempts");
    }

    public static class Profile
    {
        public static Error UpdateFailed => Error.Create("Profile.UpdateFailed", "Failed to update profile");
        public static Error AvatarUploadFailed => Error.Create("Profile.AvatarUploadFailed", "Failed to upload avatar");
        public static Error PasswordChangeFailed => Error.Create("Profile.PasswordChangeFailed", "Failed to change password");
        public static Error InvalidCurrentPassword => Error.Create("Profile.InvalidCurrentPassword", "Current password is incorrect");
    }

    public static class External
    {
        public static Error ProviderError => Error.Create("External.ProviderError", "Error from external provider");
        public static Error EmailNotProvided => Error.Create("External.EmailNotProvided", "Email not provided by external provider");
        public static Error LinkFailed => Error.Create("External.LinkFailed", "Failed to link external account");
        public static Error UnlinkFailed => Error.Create("External.UnlinkFailed", "Failed to unlink external account");
        public static Error CannotUnlinkLastLogin => Error.Create("External.CannotUnlinkLastLogin", "Cannot unlink the only login method");
        public static Error ProcessFailed => Error.Create("External.ProcessFailed", "Failed to process external login");
    }

    public static ValidationError WithIdentityErrors(string message, IEnumerable<IdentityError> errors) =>
        ValidationError.Create("Identity.ValidationFailed", message, new Dictionary<string, string[]>
        {
            ["errors"] = errors.Select(e => e.Description).ToArray()
        });

    public static ValidationError WithIdentityErrors(string code, string message, IEnumerable<string> errors) =>
        ValidationError.Create(code, message, new Dictionary<string, string[]> { ["errors"] = errors.ToArray() });
}
