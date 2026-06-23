using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Queries.GetRoleById;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetRoleByIdQuery(Guid RoleId) : IQuery<Identity.Application.Interfaces.RoleDto?>;

public class GetRoleByIdQueryHandler(
    Identity.Application.Interfaces.IApplicationDbContext context,
    ILogger<GetRoleByIdQueryHandler> logger) : IQueryHandler<GetRoleByIdQuery, Identity.Application.Interfaces.RoleDto?>
{
    public async Task<Result<Identity.Application.Interfaces.RoleDto?>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        var role = await context.AppRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.RoleId, cancellationToken);

        if (role is null)
            return Result.Success<Identity.Application.Interfaces.RoleDto?>(null);

        var permissions = await context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == query.RoleId && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);

        return Result.Success<Identity.Application.Interfaces.RoleDto?>(new Identity.Application.Interfaces.RoleDto(role.Id, role.Name, role.Description, role.IsSystemRole, permissions));
    }
}
