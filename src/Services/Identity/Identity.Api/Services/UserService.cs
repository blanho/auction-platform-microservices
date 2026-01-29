using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Identity.Api.DomainEvents;
using Identity.Api.DTOs.Audit;
using Identity.Api.DTOs.Seller;
using Identity.Api.DTOs.Users;
using Identity.Api.Errors;
using Identity.Api.Filters;
using Identity.Api.Interfaces;
using Identity.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMediator _mediator;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMediator mediator,
        IAuditPublisher auditPublisher,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mediator = mediator;
        _auditPublisher = auditPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<ApplicationUser?> FindByIdAsync(string id) =>
        _userManager.FindByIdAsync(id);

    public Task<ApplicationUser?> FindByNameAsync(string username) =>
        _userManager.FindByNameAsync(username);

    public Task<ApplicationUser?> FindByEmailAsync(string email) =>
        _userManager.FindByEmailAsync(email);

    public async Task<(ApplicationUser? User, IList<string> Roles)> GetByIdWithRolesAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return (null, Array.Empty<string>());

        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

        var roles = await _roleManager.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

        return (user, roles);
    }

    public async Task<PaginatedResult<AdminUserDto>> GetUsersPagedAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var roleId = await GetRoleIdAsync(query.Filter.Role, cancellationToken);

        var dbQuery = _userManager.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ApplyUserFilters(query.Filter.Search, query.Filter.IsActive, query.Filter.IsSuspended)
            .ApplyRoleFilter(roleId)
            .ApplySorting(query, UserSortMap.Map, u => u.CreatedAt);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var users = await dbQuery
            .ApplyPaging(query)
            .ToListAsync(cancellationToken);

        var rolesDictionary = await GetRolesDictionaryAsync(users, cancellationToken);

        return new PaginatedResult<AdminUserDto>(
            MapUsersToDto(users, rolesDictionary),
            totalCount,
            query.Page,
            query.PageSize);
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var (user, roles) = await GetByIdWithRolesAsync(id, cancellationToken);
        if (user == null)
            return null;

        var dto = _mapper.Map<AdminUserDto>(user);
        dto.Roles = roles.ToList();
        return dto;
    }

    public async Task<AdminStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        var stats = await _userManager.Users
            .GroupBy(_ => 1)
            .Select(g => new AdminStatsResponse
            {
                TotalUsers = g.Count(),
                ActiveUsers = g.Count(u => u.IsActive),
                SuspendedUsers = g.Count(u => u.IsSuspended),
                NewUsersThisMonth = g.Count(u => u.CreatedAt >= thirtyDaysAgo)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new AdminStatsResponse();
    }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, string password) =>
        _userManager.CreateAsync(user, password);

    public Task<IdentityResult> CreateWithoutPasswordAsync(ApplicationUser user) =>
        _userManager.CreateAsync(user);

    public Task<IdentityResult> UpdateAsync(ApplicationUser user) =>
        _userManager.UpdateAsync(user);

    public Task<IdentityResult> DeleteAsync(ApplicationUser user) =>
        _userManager.DeleteAsync(user);

    public Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token) =>
        _userManager.ConfirmEmailAsync(user, token);

    public Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user) =>
        _userManager.GenerateEmailConfirmationTokenAsync(user);

    public Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword) =>
        _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

    public Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword) =>
        _userManager.ResetPasswordAsync(user, token, newPassword);

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) =>
        _userManager.GeneratePasswordResetTokenAsync(user);

    public Task<IList<string>> GetRolesAsync(ApplicationUser user) =>
        _userManager.GetRolesAsync(user);

    public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role) =>
        _userManager.AddToRoleAsync(user, role);

    public Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles) =>
        _userManager.AddToRolesAsync(user, roles);

    public Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles) =>
        _userManager.RemoveFromRolesAsync(user, roles);

    public Task<bool> RoleExistsAsync(string role) =>
        _roleManager.RoleExistsAsync(role);

    public Task CreateRoleAsync(string role) =>
        _roleManager.CreateAsync(new IdentityRole(role));

    public async Task EnsureRoleExistsAsync(string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));
    }

    public async Task EnsureRolesExistAsync(IEnumerable<string> roles)
    {
        foreach (var role in roles)
            await EnsureRoleExistsAsync(role);
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user) =>
        _userManager.GetLoginsAsync(user);

    public Task<IdentityResult> AddLoginAsync(ApplicationUser user, ExternalLoginInfo info) =>
        _userManager.AddLoginAsync(user, info);

    public Task<bool> VerifyTwoFactorTokenAsync(ApplicationUser user, string tokenProvider, string token) =>
        _userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, token);

    public async Task<Result<AdminUserDto>> SuspendUserAsync(
        string userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var (user, roles) = await GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.NotFound);

        var oldUserData = UserAuditData.FromUser(user, roles);

        user.IsSuspended = true;
        user.SuspensionReason = reason;
        user.SuspendedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.SuspendFailed);

        await _mediator.Publish(new UserSuspendedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Reason = reason
        }, cancellationToken);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, roles),
            AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object> { ["action"] = "suspend", ["reason"] = reason },
            cancellationToken);

        _logger.LogWarning("User {UserId} suspended for reason: {Reason}", userId, reason);
        var dto = _mapper.Map<AdminUserDto>(user);
        dto.Roles = roles.ToList();
        return Result.Success(dto);
    }

    public async Task<Result<AdminUserDto>> UnsuspendUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var (user, roles) = await GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.NotFound);

        var oldUserData = UserAuditData.FromUser(user, roles);

        user.IsSuspended = false;
        user.SuspensionReason = null;
        user.SuspendedAt = null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.UnsuspendFailed);

        await _mediator.Publish(new UserReactivatedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!
        }, cancellationToken);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, roles),
            AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object> { ["action"] = "unsuspend" },
            cancellationToken);

        _logger.LogInformation("User {UserId} unsuspended", userId);
        var dto = _mapper.Map<AdminUserDto>(user);
        dto.Roles = roles.ToList();
        return Result.Success(dto);
    }

    public async Task<Result<AdminUserDto>> ActivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var (user, roles) = await GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.NotFound);

        var oldUserData = UserAuditData.FromUser(user, roles);
        user.IsActive = true;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.ActivateFailed);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, roles),
            AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object> { ["action"] = "activate" },
            cancellationToken);

        var dto = _mapper.Map<AdminUserDto>(user);
        dto.Roles = roles.ToList();
        return Result.Success(dto);
    }

    public async Task<Result<AdminUserDto>> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var (user, roles) = await GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.NotFound);

        var oldUserData = UserAuditData.FromUser(user, roles);
        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.DeactivateFailed);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, roles),
            AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object> { ["action"] = "deactivate" },
            cancellationToken);

        var dto = _mapper.Map<AdminUserDto>(user);
        dto.Roles = roles.ToList();
        return Result.Success(dto);
    }

    public async Task<Result<AdminUserDto>> UpdateUserRolesAsync(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var (user, currentRoles) = await GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.NotFound);

        var oldUserData = UserAuditData.FromUser(user, currentRoles);

        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var rolesList = roles.ToList();
        await EnsureRolesExistAsync(rolesList);
        await _userManager.AddToRolesAsync(user, rolesList);

        await _mediator.Publish(new UserRoleChangedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Roles = rolesList.ToArray()
        }, cancellationToken);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, rolesList),
            AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object>
            {
                ["action"] = "role_change",
                ["previousRoles"] = currentRoles.ToList(),
                ["newRoles"] = rolesList
            },
            cancellationToken);

        _logger.LogInformation("User {UserId} roles updated to: {Roles}", userId, string.Join(", ", rolesList));
        var dto = _mapper.Map<AdminUserDto>(user);
        dto.Roles = rolesList;
        return Result.Success(dto);
    }

    public async Task<Result> DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var userAuditData = UserAuditData.FromUser(user);
        var username = user.UserName!;
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.User.DeleteFailed);

        await _mediator.Publish(new UserDeletedDomainEvent
        {
            UserId = userId,
            Username = username
        }, cancellationToken);

        await _auditPublisher.PublishAsync(
            Guid.Parse(userId),
            userAuditData,
            AuditAction.Deleted,
            cancellationToken: cancellationToken);

        _logger.LogWarning("User {UserId} deleted", userId);
        return Result.Success();
    }

    public async Task<Result<SellerStatusResponse>> ApplyForSellerAsync(
        string userId,
        bool acceptTerms,
        CancellationToken cancellationToken = default)
    {
        var (user, roles) = await GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.NotFound);

        if (roles.Contains(AppRoles.Seller))
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.AlreadySeller);

        if (roles.Contains(AppRoles.Admin))
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.AdminHasSellerPrivileges);

        await EnsureRoleExistsAsync(AppRoles.Seller);

        var result = await _userManager.AddToRoleAsync(user, AppRoles.Seller);
        if (!result.Succeeded)
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.SellerUpgradeFailed);

        var updatedRoles = roles.Append(AppRoles.Seller).ToList();

        await _mediator.Publish(new UserRoleChangedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Roles = updatedRoles.ToArray()
        }, cancellationToken);

        _logger.LogInformation("User {UserId} upgraded to seller", userId);
        return Result.Success(new SellerStatusResponse
        {
            IsSeller = true,
            CanBecomeSeller = false,
            Roles = updatedRoles
        });
    }

    public async Task<Result<SellerStatusResponse>> GetSellerStatusAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var (user, roles) = await GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.NotFound);

        var isSeller = roles.Contains(AppRoles.Seller) || roles.Contains(AppRoles.Admin);

        return Result.Success(new SellerStatusResponse
        {
            IsSeller = isSeller,
            CanBecomeSeller = !isSeller && roles.Contains(AppRoles.User),
            Roles = roles.ToList()
        });
    }

    private async Task<string?> GetRoleIdAsync(string? roleName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return null;

        return await _roleManager.Roles
            .AsNoTracking()
            .Where(r => r.Name == roleName)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Dictionary<string, string>> GetRolesDictionaryAsync(
        List<ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        var roleIds = users
            .SelectMany(u => u.UserRoles)
            .Select(ur => ur.RoleId)
            .Distinct()
            .ToList();

        if (roleIds.Count == 0)
            return [];

        return await _roleManager.Roles
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name!, cancellationToken);
    }

    private List<AdminUserDto> MapUsersToDto(List<ApplicationUser> users, Dictionary<string, string> rolesDictionary)
    {
        return users.Select(user =>
        {
            var dto = _mapper.Map<AdminUserDto>(user);
            dto.Roles = user.UserRoles
                .Select(ur => rolesDictionary.GetValueOrDefault(ur.RoleId))
                .OfType<string>()
                .ToList();
            return dto;
        }).ToList();
    }
}
