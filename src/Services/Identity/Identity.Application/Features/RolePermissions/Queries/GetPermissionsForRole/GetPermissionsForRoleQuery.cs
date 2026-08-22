using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Queries.GetPermissionsForRole;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

public record GetPermissionsForRoleQuery(Guid RoleId) : IQuery<IReadOnlyList<string>>;

public class GetPermissionsForRoleQueryHandler(
    IApplicationDbContext context) : IQueryHandler<GetPermissionsForRoleQuery, IReadOnlyList<string>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(GetPermissionsForRoleQuery query, CancellationToken cancellationToken)
    {
        var permissions = await context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == query.RoleId && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<string>>(permissions);
    }
}
