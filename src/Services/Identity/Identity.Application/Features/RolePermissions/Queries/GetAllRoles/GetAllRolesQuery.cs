using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Queries.GetAllRoles;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetAllRolesQuery() : IQuery<IReadOnlyList<Identity.Application.Interfaces.RoleDto>>;

public class GetAllRolesQueryHandler(
    Identity.Application.Interfaces.IApplicationDbContext context,
    ILogger<GetAllRolesQueryHandler> logger) : IQueryHandler<GetAllRolesQuery, IReadOnlyList<Identity.Application.Interfaces.RoleDto>>
{
    public async Task<Result<IReadOnlyList<Identity.Application.Interfaces.RoleDto>>> Handle(GetAllRolesQuery query, CancellationToken cancellationToken)
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

        var result = roles.Select(r => new Identity.Application.Interfaces.RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystemRole,
            permissionsByRole.GetValueOrDefault(r.Id, [])
        )).ToList();

        return Result.Success<IReadOnlyList<Identity.Application.Interfaces.RoleDto>>(result);
    }
}
