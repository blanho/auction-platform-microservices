namespace Identity.Application.Features.Users.Commands.UnsuspendUser;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record UnsuspendUserCommand(string UserId) : ICommand<AdminUserDto>;

public class UnsuspendUserCommandHandler(
    Helpers.IUserHelper userHelper,
    ILogger<UnsuspendUserCommandHandler> logger) : ICommandHandler<UnsuspendUserCommand, AdminUserDto>
{
    public async Task<Result<AdminUserDto>> Handle(UnsuspendUserCommand command, CancellationToken cancellationToken)
    {
        var result = await userHelper.ApplyUserStatusChangeAsync(
            command.UserId,
            user => { user.IsSuspended = false; user.SuspensionReason = null; user.SuspendedAt = null; },
            IdentityErrors.User.UnsuspendFailed,
            (user, _) => new UserReactivatedDomainEvent { UserId = user.Id, Username = user.UserName! },
            new Dictionary<string, object> { [AuditMetadataKeys.ActionLower] = IdentityDefaults.Audit.Unsuspend },
            cancellationToken);

        if (result.IsSuccess)
            logger.LogInformation("User {UserId} unsuspended", command.UserId);

        return result;
    }
}
