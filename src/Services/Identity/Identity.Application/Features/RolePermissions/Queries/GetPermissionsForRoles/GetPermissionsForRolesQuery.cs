namespace Identity.Application.Features.RolePermissions.Queries.GetPermissionsForRoles;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

using Microsoft.EntityFrameworkCore;
using DefaultRolePermissions = BuildingBlocks.Application.Authorization.RolePermissions;

public record GetPermissionsForRolesQuery(IEnumerable<string> RoleNames) : IQuery<HashSet<string>>;

public class GetPermissionsForRolesQueryHandler(
    IApplicationDbContext context) : IQueryHandler<GetPermissionsForRolesQuery, HashSet<string>>
{
    public async Task<Result<HashSet<string>>> Handle(GetPermissionsForRolesQuery query, CancellationToken cancellationToken)
    {
        var roleNamesList = query.RoleNames.ToList();

        var roleIds = await context.AppRoles
            .AsNoTracking()
            .Where(r => roleNamesList.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return Result.Success(DefaultRolePermissions.GetPermissionsForRoles(roleNamesList));
        }

        var permissions = await context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => roleIds.Contains(p.RoleId) && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (permissions.Count == 0)
        {
            return Result.Success(DefaultRolePermissions.GetPermissionsForRoles(roleNamesList));
        }

        return Result.Success<HashSet<string>>([.. permissions]);
    }
}
