namespace Identity.Application.Features.Users.Commands.UpdateUserRoles;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record UpdateUserRolesCommand(string UserId, IEnumerable<string> Roles) : ICommand<AdminUserDto>;

public class UpdateUserRolesCommandHandler(
    IUserService userService,
    IMediator mediator,
    BuildingBlocks.Application.Abstractions.Auditing.IAuditPublisher auditPublisher,
    AutoMapper.IMapper mapper,
    ILogger<UpdateUserRolesCommandHandler> logger) : ICommandHandler<UpdateUserRolesCommand, AdminUserDto>
{
    public async Task<Result<AdminUserDto>> Handle(UpdateUserRolesCommand command, CancellationToken cancellationToken)
    {
        var (user, currentRoles) = await userService.GetByIdWithRolesAsync(command.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<AdminUserDto>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var oldUserData = Identity.Application.DTOs.Audit.UserAuditData.FromUser(user, currentRoles);

        await userService.RemoveFromRolesAsync(user, currentRoles);

        var rolesList = command.Roles.ToList();
        foreach (var role in rolesList)
            await userService.EnsureRoleExistsAsync(role);
        await userService.AddToRolesAsync(user, rolesList);

        await mediator.Publish(new Identity.Domain.Events.UserRoleChangedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Roles = rolesList.ToArray()
        }, cancellationToken);

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            Identity.Application.DTOs.Audit.UserAuditData.FromUser(user, rolesList),
            BuildingBlocks.Application.Abstractions.Auditing.AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object>
            {
                [BuildingBlocks.Application.Constants.AuditMetadataKeys.ActionLower] = Identity.Domain.Constants.IdentityDefaults.Audit.RoleChange,
                [BuildingBlocks.Application.Constants.AuditMetadataKeys.PreviousRoles] = currentRoles.ToList(),
                [BuildingBlocks.Application.Constants.AuditMetadataKeys.NewRoles] = rolesList
            },
            cancellationToken);

        logger.LogInformation("User {UserId} roles updated to: {Roles}", command.UserId, string.Join(", ", rolesList));
        var dto = mapper.Map<AdminUserDto>(user);
        dto.Roles = rolesList;
        return Result.Success(dto);
    }
}
