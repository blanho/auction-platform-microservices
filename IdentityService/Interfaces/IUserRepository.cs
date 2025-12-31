using IdentityService.DTOs;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(List<ApplicationUser> Users, int TotalCount)> GetPagedAsync(
        string? search = null,
        string? role = null,
        bool? isActive = null,
        bool? isSuspended = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);
    Task<bool> RoleExistsAsync(string role);
    Task CreateRoleAsync(string role);
    Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
