using BuildingBlocks.Application.Abstractions;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Interfaces;

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
