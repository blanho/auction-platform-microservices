namespace Identity.Application.Features.Users.Commands.DeactivateUser;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record DeactivateUserCommand(string UserId) : ICommand<AdminUserDto>;

public class DeactivateUserCommandHandler(
    Helpers.IUserHelper userHelper) : ICommandHandler<DeactivateUserCommand, AdminUserDto>
{
    public async Task<Result<AdminUserDto>> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        return await userHelper.ApplyUserStatusChangeAsync(
            command.UserId,
            user => user.IsActive = false,
            IdentityErrors.User.DeactivateFailed,
            (_, _) => null,
            new Dictionary<string, object> { [AuditMetadataKeys.ActionLower] = IdentityDefaults.Audit.Deactivate },
            cancellationToken);
    }
}
