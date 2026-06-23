namespace Identity.Application.Features.Users.Commands.SuspendUser;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record SuspendUserCommand(string UserId, string Reason) : ICommand<AdminUserDto>;

public class SuspendUserCommandHandler(
    Identity.Application.Features.Users.Helpers.IUserHelper userHelper,
    ILogger<SuspendUserCommandHandler> logger) : ICommandHandler<SuspendUserCommand, AdminUserDto>
{
    public async Task<Result<AdminUserDto>> Handle(SuspendUserCommand command, CancellationToken cancellationToken)
    {
        var result = await userHelper.ApplyUserStatusChangeAsync(
            command.UserId,
            user => { user.IsSuspended = true; user.SuspensionReason = command.Reason; user.SuspendedAt = DateTimeOffset.UtcNow; },
            IdentityErrors.User.SuspendFailed,
            (user, _) => new UserSuspendedDomainEvent { UserId = user.Id, Username = user.UserName!, Reason = command.Reason },
            new Dictionary<string, object> { [BuildingBlocks.Application.Constants.AuditMetadataKeys.ActionLower] = Identity.Domain.Constants.IdentityDefaults.Audit.Suspend, [BuildingBlocks.Application.Constants.AuditMetadataKeys.ReasonLower] = command.Reason },
            cancellationToken);

        if (result.IsSuccess)
            logger.LogWarning("User {UserId} suspended for reason: {Reason}", command.UserId, command.Reason);

        return result;
    }
}
