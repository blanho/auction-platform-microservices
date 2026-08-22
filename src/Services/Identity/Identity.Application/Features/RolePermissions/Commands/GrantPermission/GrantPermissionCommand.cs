using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Commands.GrantPermission;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

public record GrantPermissionCommand(Guid RoleId, string Permission) : ICommand<bool>;

public class GrantPermissionCommandHandler(
    IApplicationDbContext context) : ICommandHandler<GrantPermissionCommand, bool>
{
    public async Task<Result<bool>> Handle(GrantPermissionCommand command, CancellationToken cancellationToken)
    {
        var role = await context.AppRoles.FindAsync([command.RoleId], cancellationToken);
        if (role is null)
            return Result.Success(false);

        var existing = await context.RolePermissionStrings
            .FirstOrDefaultAsync(p => p.RoleId == command.RoleId && p.PermissionCode == command.Permission, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsEnabled)
                return Result.Success(true);

            existing.IsEnabled = true;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            context.RolePermissionStrings.Add(new RolePermissionString
            {
                Id = Guid.NewGuid(),
                RoleId = command.RoleId,
                PermissionCode = command.Permission,
                IsEnabled = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
