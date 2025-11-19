using BuildingBlocks.Application.Abstractions;
using Identity.Api.DTOs.Seller;
using Identity.Api.DTOs.Users;
using Identity.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Interfaces;

public interface IUserService
{
    Task<ApplicationUser?> FindByIdAsync(string id);
    Task<(ApplicationUser? User, IList<string> Roles)> GetByIdWithRolesAsync(string id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> FindByNameAsync(string username);
    Task<ApplicationUser?> FindByEmailAsync(string email);

    Task<PaginatedResult<AdminUserDto>> GetUsersPagedAsync(GetUsersQuery query, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityResult> CreateWithoutPasswordAsync(ApplicationUser user);
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);

    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);

    Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task<bool> RoleExistsAsync(string role);
    Task CreateRoleAsync(string role);

    Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user);
    Task<IdentityResult> AddLoginAsync(ApplicationUser user, ExternalLoginInfo info);

    Task<bool> VerifyTwoFactorTokenAsync(ApplicationUser user, string tokenProvider, string token);

    Task<AdminStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default);

    Task<Result<AdminUserDto>> SuspendUserAsync(string userId, string reason, CancellationToken cancellationToken = default);
    Task<Result<AdminUserDto>> UnsuspendUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<AdminUserDto>> ActivateUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<AdminUserDto>> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<AdminUserDto>> UpdateUserRolesAsync(string userId, IEnumerable<string> roles, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<SellerStatusResponse>> ApplyForSellerAsync(string userId, bool acceptTerms, CancellationToken cancellationToken = default);
    Task<Result<SellerStatusResponse>> GetSellerStatusAsync(string userId, CancellationToken cancellationToken = default);
}

