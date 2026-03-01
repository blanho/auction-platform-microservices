using System.Security.Claims;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Exceptions;
using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Identity.Api;

public static class SeedData
{
    private const string DefaultSeedPassword = "SeedUser@Dev123!";

    public static async Task EnsureSeedDataAsync(WebApplication app)
    {
        await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var seedPassword = configuration["SeedData:Password"] ?? DefaultSeedPassword;

        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await EnsureRolesExistAsync(roleMgr);
        await EnsureRolePermissionsAsync(dbContext);

        await CreateAdminUserAsync(userMgr, seedPassword);
        await CreateSellerUserAsync(userMgr, seedPassword);
        await CreateBuyerUsersAsync(userMgr, seedPassword);
    }

    private static async Task EnsureRolesExistAsync(RoleManager<IdentityRole> roleMgr)
    {
        foreach (var roleName in Roles.AllRoles)
        {
            if (!await roleMgr.RoleExistsAsync(roleName))
            {
                var result = await roleMgr.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new ConfigurationException($"Failed to create role {roleName}: {result.Errors.First().Description}");
                }
                Log.Debug("{Role} role created", roleName);
            }
        }
    }

    private static async Task EnsureRolePermissionsAsync(ApplicationDbContext dbContext)
    {
        var rolePermissionMap = new Dictionary<string, HashSet<string>>
        {
            [Roles.User] = RolePermissions.GetPermissionsForRole(Roles.User),
            [Roles.Seller] = RolePermissions.GetPermissionsForRole(Roles.Seller),
            [Roles.Admin] = RolePermissions.GetPermissionsForRole(Roles.Admin),
        };

        foreach (var (roleName, permissions) in rolePermissionMap)
        {
            var appRole = await dbContext.AppRoles
                .FirstOrDefaultAsync(r => r.Name == roleName);
            
            if (appRole == null)
            {
                Log.Warning("AppRole {RoleName} not found, skipping permission seeding", roleName);
                continue;
            }

            var existingPermissions = await dbContext.RolePermissionStrings
                .Where(rp => rp.RoleId == appRole.Id)
                .Select(rp => rp.PermissionCode)
                .ToListAsync();

            if (existingPermissions.Count > 0)
            {
                Log.Debug("Role {RoleName} already has {Count} permissions, skipping seeding", roleName, existingPermissions.Count);
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            var permissionEntities = permissions.Select(permission => new RolePermissionString
            {
                RoleId = appRole.Id,
                PermissionCode = permission,
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now,
            });

            await dbContext.RolePermissionStrings.AddRangeAsync(permissionEntities);
            Log.Debug("Seeded {Count} permissions for role {RoleName}", permissions.Count, roleName);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task CreateAdminUserAsync(UserManager<ApplicationUser> userMgr, string seedPassword)
    {
        var admin = await userMgr.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@auction.com",
                EmailConfirmed = true,
                FullName = "System Administrator",
            };
            var result = await userMgr.CreateAsync(admin, seedPassword);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddToRoleAsync(admin, Roles.Admin);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(admin, new Claim[]
            {
                new(ClaimTypes.Name, "System Administrator"),
                new(ClaimTypes.Role, Roles.Admin),
            });
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }
            Log.Debug("admin user created");
        }
        else
        {
            Log.Debug("admin user already exists");
        }
    }

    private static async Task CreateSellerUserAsync(UserManager<ApplicationUser> userMgr, string seedPassword)
    {
        var seller = await userMgr.FindByNameAsync("seller");
        if (seller == null)
        {
            seller = new ApplicationUser
            {
                UserName = "seller",
                Email = "seller@auction.com",
                EmailConfirmed = true,
                FullName = "Demo Seller",
            };
            var result = await userMgr.CreateAsync(seller, seedPassword);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddToRoleAsync(seller, Roles.Seller);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(seller, new Claim[]
            {
                new(ClaimTypes.Name, "Demo Seller"),
                new(ClaimTypes.Role, Roles.Seller),
            });
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }
            Log.Debug("seller user created");
        }
        else
        {
            Log.Debug("seller user already exists");
        }
    }

    private static async Task CreateBuyerUsersAsync(UserManager<ApplicationUser> userMgr, string seedPassword)
    {
        await CreateBuyerUserAsync(userMgr, seedPassword, "alice", "alice@example.com", "Alice Smith");
        await CreateBuyerUserAsync(userMgr, seedPassword, "bob", "bob@example.com", "Bob Johnson");
    }

    private static async Task CreateBuyerUserAsync(
        UserManager<ApplicationUser> userMgr, 
        string seedPassword,
        string username,
        string email,
        string fullName)
    {
        var user = await userMgr.FindByNameAsync(username);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
            };
            var result = await userMgr.CreateAsync(user, seedPassword);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddToRoleAsync(user, Roles.User);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(user, new Claim[]
            {
                new(ClaimTypes.Name, fullName),
                new(ClaimTypes.Role, Roles.User),
            });
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }
            Log.Debug("{Username} user created", username);
        }
        else
        {
            Log.Debug("{Username} user already exists", username);
        }
    }
}
