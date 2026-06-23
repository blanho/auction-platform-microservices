namespace Identity.Application.Features.Users.Commands.DeleteUser;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record DeleteUserCommand(string UserId) : ICommand;

public class DeleteUserCommandHandler(
    IUserService userService,
    IMediator mediator,
    BuildingBlocks.Application.Abstractions.Auditing.IAuditPublisher auditPublisher,
    ILogger<DeleteUserCommandHandler> logger) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var userAuditData = Identity.Application.DTOs.Audit.UserAuditData.FromUser(user);
        var username = user.UserName!;
        
        var result = await userService.DeleteAsync(user);
        if (!result.Succeeded)
            return Result.Failure(Identity.Application.Errors.IdentityErrors.User.DeleteFailed);

        await mediator.Publish(new Identity.Domain.Events.UserDeletedDomainEvent
        {
            UserId = command.UserId,
            Username = username
        }, cancellationToken);

        await auditPublisher.PublishAsync(
            Guid.Parse(command.UserId),
            userAuditData,
            BuildingBlocks.Application.Abstractions.Auditing.AuditAction.Deleted,
            cancellationToken: cancellationToken);

        logger.LogWarning("User {UserId} deleted", command.UserId);
        return Result.Success();
    }
}
