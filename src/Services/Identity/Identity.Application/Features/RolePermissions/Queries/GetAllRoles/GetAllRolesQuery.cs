using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Queries.GetAllRoles;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

public record GetAllRolesQuery() : IQuery<IReadOnlyList<RoleDto>>;

public class GetAllRolesQueryHandler(
    IApplicationDbContext context) : IQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>>
{
    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(GetAllRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await context.AppRoles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();
        var permissionStrings = await context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => roleIds.Contains(p.RoleId) && p.IsEnabled)
            .ToListAsync(cancellationToken);

        var permissionsByRole = permissionStrings
            .GroupBy(p => p.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.PermissionCode).ToList());

        var result = roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystemRole,
            permissionsByRole.GetValueOrDefault(r.Id, [])
        )).ToList();

        return Result.Success<IReadOnlyList<RoleDto>>(result);
    }
}
