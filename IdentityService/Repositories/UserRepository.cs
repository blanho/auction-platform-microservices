using IdentityService.DTOs;
using IdentityService.Interfaces;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<(List<ApplicationUser> Users, int TotalCount)> GetPagedAsync(
        string? search = null,
        string? role = null,
        bool? isActive = null,
        bool? isSuspended = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(searchLower) ||
                u.Email!.ToLower().Contains(searchLower) ||
                (u.FullName != null && u.FullName.ToLower().Contains(searchLower)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (isSuspended.HasValue)
        {
            query = query.Where(u => u.IsSuspended == isSuspended.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(role))
        {
            var filteredUsers = new List<ApplicationUser>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(role))
                {
                    filteredUsers.Add(user);
                }
            }
            return (filteredUsers, totalCount);
        }

        return (users, totalCount);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        return await _userManager.AddToRolesAsync(user, roles);
    }

    public async Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        return await _userManager.RemoveFromRolesAsync(user, roles);
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<bool> RoleExistsAsync(string role)
    {
        return await _roleManager.RoleExistsAsync(role);
    }

    public async Task CreateRoleAsync(string role)
    {
        await _roleManager.CreateAsync(new IdentityRole(role));
    }

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive, cancellationToken);
        var suspendedUsers = await _userManager.Users.CountAsync(u => u.IsSuspended, cancellationToken);
        var newUsersThisMonth = await _userManager.Users
            .CountAsync(u => u.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30), cancellationToken);

        return new AdminStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            SuspendedUsers = suspendedUsers,
            NewUsersThisMonth = newUsersThisMonth
        };
    }
}
