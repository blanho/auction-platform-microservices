using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Commands.SetPermissions;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

public record SetPermissionsCommand(Guid RoleId, IEnumerable<string> Permissions) : ICommand<bool>;

public class SetPermissionsCommandHandler(
    IApplicationDbContext context) : ICommandHandler<SetPermissionsCommand, bool>
{
    public async Task<Result<bool>> Handle(SetPermissionsCommand command, CancellationToken cancellationToken)
    {
        var role = await context.AppRoles.FindAsync([command.RoleId], cancellationToken);
        if (role is null)
            return Result.Success(false);

        var permissionSet = command.Permissions.ToHashSet();

        var existing = await context.RolePermissionStrings
            .Where(p => p.RoleId == command.RoleId)
            .ToListAsync(cancellationToken);

        foreach (var perm in existing)
        {
            perm.IsEnabled = permissionSet.Contains(perm.PermissionCode);
            perm.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var existingCodes = existing.Select(p => p.PermissionCode).ToHashSet();
        var newPermissions = permissionSet.Except(existingCodes);

        foreach (var code in newPermissions)
        {
            context.RolePermissionStrings.Add(new RolePermissionString
            {
                Id = Guid.NewGuid(),
                RoleId = command.RoleId,
                PermissionCode = code,
                IsEnabled = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
