using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Queries.GetRoleById;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

public record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleDto?>;

public class GetRoleByIdQueryHandler(
    IApplicationDbContext context) : IQueryHandler<GetRoleByIdQuery, RoleDto?>
{
    public async Task<Result<RoleDto?>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        var role = await context.AppRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.RoleId, cancellationToken);

        if (role is null)
            return Result.Success<RoleDto?>(null);

        var permissions = await context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == query.RoleId && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);

        return Result.Success<RoleDto?>(new RoleDto(role.Id, role.Name, role.Description, role.IsSystemRole, permissions));
    }
}
