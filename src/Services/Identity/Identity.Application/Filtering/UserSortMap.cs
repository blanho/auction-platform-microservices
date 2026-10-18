using System.Linq.Expressions;
using BuildingBlocks.Application.Paging;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Filtering;

public static class UserSortMap
{
    public static IReadOnlyDictionary<string, Expression<Func<ApplicationUser, object>>> Map { get; } =
        new SortMapBuilder<ApplicationUser, AdminUserDto>()
            .Add(field => field.CreatedAt, user => user.CreatedAt)
            .Add(field => field.Username, user => user.UserName!)
            .Add(field => field.Email, user => user.Email!)
            .Add(field => field.FullName, user => user.FullName ?? string.Empty)
            .Add(field => field.LastLoginAt, user => user.LastLoginAt ?? DateTimeOffset.MinValue)
            .Add(field => field.IsActive, user => user.IsActive)
            .Add(field => field.IsSuspended, user => user.IsSuspended)
            .Build();
}
