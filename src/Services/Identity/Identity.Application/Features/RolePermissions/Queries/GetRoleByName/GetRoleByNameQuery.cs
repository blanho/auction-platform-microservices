using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Queries.GetRoleByName;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

public record GetRoleByNameQuery(string RoleName) : IQuery<RoleDto?>;

public class GetRoleByNameQueryHandler(
    IApplicationDbContext context) : IQueryHandler<GetRoleByNameQuery, RoleDto?>
{
    public async Task<Result<RoleDto?>> Handle(GetRoleByNameQuery query, CancellationToken cancellationToken)
    {
        var role = await context.AppRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == query.RoleName, cancellationToken);

        if (role is null)
            return Result.Success<RoleDto?>(null);

        var permissions = await context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == role.Id && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);

        return Result.Success<RoleDto?>(new RoleDto(role.Id, role.Name, role.Description, role.IsSystemRole, permissions));
    }
}
