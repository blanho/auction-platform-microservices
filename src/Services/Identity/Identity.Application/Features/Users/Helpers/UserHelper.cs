namespace Identity.Application.Features.Users.Helpers;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Identity.Application.DTOs.Users;
using Identity.Application.Errors;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Identity.Application.DTOs.Audit;
using Identity.Domain.Constants;
using BuildingBlocks.Application.Constants;

public interface IUserHelper
{
    Task<Result<AdminUserDto>> ApplyUserStatusChangeAsync(
        string userId,
        Action<ApplicationUser> applyChange,
        Error updateFailedError,
        Func<ApplicationUser, IList<string>, INotification?> createDomainEvent,
        Dictionary<string, object> auditMetadata,
        CancellationToken cancellationToken);
}

public class UserHelper(
    IUserService userService,
    IMediator mediator,
    IAuditPublisher auditPublisher,
    IMapper mapper) : IUserHelper
{
    public async Task<Result<AdminUserDto>> ApplyUserStatusChangeAsync(
        string userId,
        Action<ApplicationUser> applyChange,
        Error updateFailedError,
        Func<ApplicationUser, IList<string>, INotification?> createDomainEvent,
        Dictionary<string, object> auditMetadata,
        CancellationToken cancellationToken)
    {
        var (user, roles) = await userService.GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<AdminUserDto>(IdentityErrors.User.NotFound);

        var previousState = UserAuditData.FromUser(user, roles);
        applyChange(user);

        var updateResult = await userService.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result.Failure<AdminUserDto>(updateFailedError);

        var domainEvent = createDomainEvent(user, roles);
        if (domainEvent != null)
            await mediator.Publish(domainEvent, cancellationToken);

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, roles),
            AuditAction.Updated,
            previousState,
            auditMetadata,
            cancellationToken);

        var dto = mapper.Map<AdminUserDto>(user);
        dto.Roles = roles.ToList();
        return Result.Success(dto);
    }
}
