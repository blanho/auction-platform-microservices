using System.Security.Claims;
using BuildingBlocks.Web.Exceptions;
using Identity.Api.Models;
using Microsoft.AspNetCore.Identity;
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

        await EnsureRolesExistAsync(roleMgr);

        await CreateAdminUserAsync(userMgr, seedPassword);
        await CreateSellerUserAsync(userMgr, seedPassword);
        await CreateBuyerUsersAsync(userMgr, seedPassword);
    }

    private static async Task EnsureRolesExistAsync(RoleManager<IdentityRole> roleMgr)
    {
        foreach (var roleName in AppRoles.AllRoles)
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

            result = await userMgr.AddToRoleAsync(admin, AppRoles.Admin);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(admin, new Claim[]
            {
                new(ClaimTypes.Name, "System Administrator"),
                new(ClaimTypes.Role, AppRoles.Admin),
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

            result = await userMgr.AddToRoleAsync(seller, AppRoles.Seller);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(seller, new Claim[]
            {
                new(ClaimTypes.Name, "Demo Seller"),
                new(ClaimTypes.Role, AppRoles.Seller),
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

            result = await userMgr.AddToRoleAsync(user, AppRoles.User);
            if (!result.Succeeded)
            {
                throw new ConfigurationException(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(user, new Claim[]
            {
                new(ClaimTypes.Name, fullName),
                new(ClaimTypes.Role, AppRoles.User),
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
