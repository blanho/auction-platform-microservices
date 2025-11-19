using BuildingBlocks.Application.Filtering;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Filters;

public sealed class SearchUserFilter(string? search) : IFilter<ApplicationUser>
{
    public IQueryable<ApplicationUser> Apply(IQueryable<ApplicationUser> query)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var pattern = $"%{search}%";
        return query.Where(u =>
            EF.Functions.ILike(u.UserName!, pattern) ||
            EF.Functions.ILike(u.Email!, pattern) ||
            (u.FullName != null && EF.Functions.ILike(u.FullName, pattern)));
    }
}

public sealed class StatusUserFilter(bool? isActive, bool? isSuspended) : IFilter<ApplicationUser>
{
    public IQueryable<ApplicationUser> Apply(IQueryable<ApplicationUser> query)
    {
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (isSuspended.HasValue)
            query = query.Where(u => u.IsSuspended == isSuspended.Value);

        return query;
    }
}

public sealed class RoleUserFilter(string? roleId) : IFilter<ApplicationUser>
{
    public IQueryable<ApplicationUser> Apply(IQueryable<ApplicationUser> query)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return query;

        return query.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId));
    }
}
