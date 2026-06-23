namespace Identity.Application.Features.Users.Commands.ActivateUser;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record ActivateUserCommand(string UserId) : ICommand<AdminUserDto>;

public class ActivateUserCommandHandler(
    Identity.Application.Features.Users.Helpers.IUserHelper userHelper) : ICommandHandler<ActivateUserCommand, AdminUserDto>
{
    public async Task<Result<AdminUserDto>> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        return await userHelper.ApplyUserStatusChangeAsync(
            command.UserId,
            user => user.IsActive = true,
            IdentityErrors.User.ActivateFailed,
            (_, _) => null,
            new Dictionary<string, object> { [BuildingBlocks.Application.Constants.AuditMetadataKeys.ActionLower] = Identity.Domain.Constants.IdentityDefaults.Audit.Activate },
            cancellationToken);
    }
}
