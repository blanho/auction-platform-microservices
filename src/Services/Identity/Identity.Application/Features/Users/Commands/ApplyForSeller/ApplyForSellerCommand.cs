namespace Identity.Application.Features.Users.Commands.ApplyForSeller;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record ApplyForSellerCommand(string UserId, bool AcceptTerms) : ICommand<SellerStatusResponse>;

public class ApplyForSellerCommandHandler(
    IUserService userService,
    IMediator mediator,
    ILogger<ApplyForSellerCommandHandler> logger) : ICommandHandler<ApplyForSellerCommand, SellerStatusResponse>
{
    public async Task<Result<SellerStatusResponse>> Handle(ApplyForSellerCommand command, CancellationToken cancellationToken)
    {
        var (user, roles) = await userService.GetByIdWithRolesAsync(command.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.NotFound);

        if (roles.Contains(Roles.Seller))
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.AlreadySeller);

        if (roles.Contains(Roles.Admin))
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.AdminHasSellerPrivileges);

        await userService.EnsureRoleExistsAsync(Roles.Seller);

        var result = await userService.AddToRoleAsync(user, Roles.Seller);
        if (!result.Succeeded)
            return Result.Failure<SellerStatusResponse>(IdentityErrors.User.SellerUpgradeFailed);

        var updatedRoles = roles.Append(Roles.Seller).ToList();

        await mediator.Publish(new UserRoleChangedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Roles = updatedRoles.ToArray()
        }, cancellationToken);

        logger.LogInformation("User {UserId} upgraded to seller", command.UserId);
        return Result.Success(new SellerStatusResponse
        {
            IsSeller = true,
            CanBecomeSeller = false,
            Roles = updatedRoles
        });
    }
}
