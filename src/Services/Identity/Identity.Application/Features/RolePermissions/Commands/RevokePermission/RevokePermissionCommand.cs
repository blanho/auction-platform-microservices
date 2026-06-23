using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.RolePermissions.Commands.RevokePermission;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record RevokePermissionCommand(Guid RoleId, string Permission) : ICommand<bool>;

public class RevokePermissionCommandHandler(
    Identity.Application.Interfaces.IApplicationDbContext context,
    ILogger<RevokePermissionCommandHandler> logger) : ICommandHandler<RevokePermissionCommand, bool>
{
    public async Task<Result<bool>> Handle(RevokePermissionCommand command, CancellationToken cancellationToken)
    {
        var existing = await context.RolePermissionStrings
            .FirstOrDefaultAsync(p => p.RoleId == command.RoleId && p.PermissionCode == command.Permission, cancellationToken);

        if (existing is null)
            return Result.Success(true);

        existing.IsEnabled = false;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
