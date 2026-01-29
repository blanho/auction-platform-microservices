using BuildingBlocks.Application.Filtering;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Filters;

public static class UserFilterExtensions
{
    public static IQueryable<ApplicationUser> ApplyUserFilters(
        this IQueryable<ApplicationUser> query,
        string? search,
        bool? isActive,
        bool? isSuspended)
    {
        var filterBuilder = FilterBuilder<ApplicationUser>.Create()
            .WhenNotEmpty(search, u =>
                EF.Functions.ILike(u.UserName!, $"%{search}%") ||
                EF.Functions.ILike(u.Email!, $"%{search}%") ||
                (u.FullName != null && EF.Functions.ILike(u.FullName, $"%{search}%")))
            .WhenHasValue(isActive, u => u.IsActive == isActive!.Value)
            .WhenHasValue(isSuspended, u => u.IsSuspended == isSuspended!.Value);

        return filterBuilder.Apply(query);
    }

    public static IQueryable<ApplicationUser> ApplyRoleFilter(
        this IQueryable<ApplicationUser> query,
        string? roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return query;

        return query.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId));
    }
}
