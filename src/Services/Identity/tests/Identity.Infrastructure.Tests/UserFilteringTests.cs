using AutoMapper;
using Identity.Application.Features.Users.Queries.GetUsers;
using Identity.Application.Filtering;
using Identity.Application.Services;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using BuildingBlocks.Application.Paging;
using TestSupport;
using Xunit;

namespace Identity.Infrastructure.Tests;

public sealed class UserFilteringTests
{
    [PostgresFact]
    public async Task HandlerAndService_CombineFiltersBeforeCountAndPagingAndReturnOnlySelectedUsersRoles()
    {
        await using var host = await UserHost.CreateAsync();
        var service = host.Service();
        var handler = new GetUsersListQueryHandler(service);
        UserQueryParams Query(int page) => new()
        {
            Role = "Seller", Page = page, PageSize = 1,
            Filter = new UserFilter { Search = "AL", IsActive = true, IsSuspended = false }
        };

        var first = await handler.Handle(new GetUsersListQuery(Query(1)), default);
        var second = await service.GetUsersPagedAsync(Query(2));
        Assert.True(first.IsSuccess);
        Assert.Equal(2, first.Value!.TotalCount);
        Assert.Equal(2, second.TotalCount);
        Assert.Equal(new[] { "alice", "albert" }, first.Value.Items.Concat(second.Items).Select(user => user.Username));
        Assert.All(first.Value.Items.Concat(second.Items), user => Assert.Equal(new[] { "Seller" }, user.Roles));
        Assert.True(first.Value.HasNextPage);
        Assert.True(second.HasPreviousPage);
        Assert.Empty(host.Db.ChangeTracker.Entries());
    }

    [PostgresFact]
    public async Task UnknownRole_ReturnsEmptyBoundedPageWhileOmittedRoleDoesNotRestrictUsers()
    {
        await using var host = await UserHost.CreateAsync();
        var service = host.Service();
        var missing = await service.GetUsersPagedAsync(new UserQueryParams { Role = "Unknown", Page = 0, PageSize = 500 });
        Assert.Empty(missing.Items);
        Assert.Equal(0, missing.TotalCount);
        Assert.Equal(1, missing.Page);
        Assert.Equal(100, missing.PageSize);
        Assert.Equal(7, (await service.GetUsersPagedAsync(new UserQueryParams { Role = "  " })).TotalCount);
        Assert.Equal(7, (await service.GetUsersPagedAsync(new UserQueryParams { Filter = null! })).TotalCount);
        var byId = await service.GetUsersPagedAsync(new UserQueryParams { Filter = new UserFilter { RoleId = "admin" } });
        Assert.Equal("alice-admin", Assert.Single(byId.Items).Username);
        Assert.Empty(host.Db.ChangeTracker.Entries());
    }

    [PostgresFact]
    public async Task Search_MatchesUsernameEmailAndNullableFullNameCaseInsensitively()
    {
        await using var host = await UserHost.CreateAsync();
        var service = host.Service();
        var fullName = await service.GetUsersPagedAsync(new UserQueryParams { Filter = new UserFilter { Search = "northSIDE" } });
        Assert.Equal("unassigned", Assert.Single(fullName.Items).Username);
        var email = await service.GetUsersPagedAsync(new UserQueryParams { Filter = new UserFilter { Search = "SPECIAL@MARKET" } });
        Assert.Equal("albert", Assert.Single(email.Items).Username);
        var nullableFields = await service.GetUsersPagedAsync(new UserQueryParams { Filter = new UserFilter { Search = "hidden fields" } });
        Assert.Equal("07", Assert.Single(nullableFields.Items).Id);
        Assert.Equal(7, (await service.GetUsersPagedAsync(new UserQueryParams { Filter = new UserFilter { Search = "  " } })).TotalCount);
        Assert.Empty(host.Db.ChangeTracker.Entries());
    }

    [PostgresFact]
    public async Task Listing_UsesSharedMultiSortBoundsAndPropagatesCancellation()
    {
        await using var host = await UserHost.CreateAsync();
        var service = host.Service();
        var sorted = await service.GetUsersPagedAsync(new UserQueryParams
        {
            PageSize = 2, SortBy = "createdAt", SortDescending = true,
            Sorts = [new SortDescriptor { Field = "USERNAME", Desc = false }]
        });
        Assert.Equal(7, sorted.TotalCount);
        Assert.Equal(new[] { "albert", "alex" }, sorted.Items.Select(user => user.Username));
        var huge = await service.GetUsersPagedAsync(new UserQueryParams { Page = int.MaxValue, PageSize = 100 });
        Assert.Empty(huge.Items);
        Assert.Equal(7, huge.TotalCount);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetUsersPagedAsync(
            new UserQueryParams { Role = "Seller" }, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new GetUsersListQueryHandler(service).Handle(
            new GetUsersListQuery(new UserQueryParams()), cancellation.Token));
        Assert.Empty(host.Db.ChangeTracker.Entries());
    }

    private sealed class UserHost(PostgresTestDatabase database, ServiceProvider services, AsyncServiceScope scope) : IAsyncDisposable
    {
        public ApplicationDbContext Db => scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        public UserService Service()
        {
            var mapper = TestProxy.Create<IMapper>((_, args) =>
            {
                var user = (ApplicationUser)args![0]!;
                return new AdminUserDto
                {
                    Id = user.Id, Username = user.UserName ?? string.Empty, Email = user.Email ?? string.Empty,
                    FullName = user.FullName, CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive, IsSuspended = user.IsSuspended
                };
            });
            return new UserService(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(), null!, null!, mapper,
                NullLogger<UserService>.Instance);
        }

        public static async Task<UserHost> CreateAsync()
        {
            var database = await PostgresTestDatabase.CreateAsync();
            var services = new ServiceCollection().AddLogging();
            services.AddDbContext<ApplicationDbContext>(builder => builder.UseNpgsql(database.ConnectionString));
            services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
            var provider = services.BuildServiceProvider();
            var host = new UserHost(database, provider, provider.CreateAsyncScope());
            try
            {
                await host.Db.Database.MigrateAsync();
                var users = new[]
                {
                    User("01", "alice", "Alice Example"), User("02", "albert", "Albert Trader"),
                    User("03", "alex", null), User("04", "alina", null), User("05", "alice-admin", null),
                    User("06", "unassigned", "Northside Seller"), User("07", null, "Hidden Fields")
                };
                users[1].Email = "special@market.test";
                users[2].IsActive = false;
                users[3].IsSuspended = true;
                host.Db.Users.AddRange(users);
                host.Db.Roles.AddRange(new IdentityRole { Id = "seller", Name = "Seller", NormalizedName = "SELLER" },
                    new IdentityRole { Id = "admin", Name = "Admin", NormalizedName = "ADMIN" });
                host.Db.UserRoles.AddRange(users.Take(5).Select(user => new IdentityUserRole<string>
                    { UserId = user.Id, RoleId = user.Id == "05" ? "admin" : "seller" }));
                await host.Db.SaveChangesAsync();
                host.Db.ChangeTracker.Clear();
                return host;
            }
            catch
            {
                await host.DisposeAsync();
                throw;
            }
        }

        private static ApplicationUser User(string id, string? username, string? fullName) => new()
        {
            Id = id, UserName = username, NormalizedUserName = username?.ToUpperInvariant(),
            Email = username is null ? null : username + "@market.test", FullName = fullName,
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        public async ValueTask DisposeAsync()
        {
            await scope.DisposeAsync();
            await services.DisposeAsync();
            await database.DisposeAsync();
        }
    }
}
