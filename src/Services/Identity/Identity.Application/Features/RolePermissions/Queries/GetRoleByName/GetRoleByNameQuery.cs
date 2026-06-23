using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Queries.GetRoleByName;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetRoleByNameQuery(string RoleName) : IQuery<Identity.Application.Interfaces.RoleDto?>;

public class GetRoleByNameQueryHandler(
    Identity.Application.Interfaces.IApplicationDbContext context,
    ILogger<GetRoleByNameQueryHandler> logger) : IQueryHandler<GetRoleByNameQuery, Identity.Application.Interfaces.RoleDto?>
{
    public async Task<Result<Identity.Application.Interfaces.RoleDto?>> Handle(GetRoleByNameQuery query, CancellationToken cancellationToken)
    {
        var role = await context.AppRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == query.RoleName, cancellationToken);

        if (role is null)
            return Result.Success<Identity.Application.Interfaces.RoleDto?>(null);

        var permissions = await context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == role.Id && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);

        return Result.Success<Identity.Application.Interfaces.RoleDto?>(new Identity.Application.Interfaces.RoleDto(role.Id, role.Name, role.Description, role.IsSystemRole, permissions));
    }
}
