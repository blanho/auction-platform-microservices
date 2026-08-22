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
    IAuditPublisher auditPublisher,
    ILogger<DeleteUserCommandHandler> logger) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var userAuditData = UserAuditData.FromUser(user);
        var username = user.UserName!;

        var result = await userService.DeleteAsync(user);
        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.User.DeleteFailed);

        await mediator.Publish(new UserDeletedDomainEvent
        {
            UserId = command.UserId,
            Username = username
        }, cancellationToken);

        await auditPublisher.PublishAsync(
            Guid.Parse(command.UserId),
            userAuditData,
            AuditAction.Deleted,
            cancellationToken: cancellationToken);

        logger.LogWarning("User {UserId} deleted", command.UserId);
        return Result.Success();
    }
}
