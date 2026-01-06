using System.Security.Claims;
using Common.Core.Authorization;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

public class SeedData
{
    private const string DefaultSeedPassword = "SeedUser@Dev123!";
    
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var seedPassword = configuration["SeedData:Password"] ?? DefaultSeedPassword;

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            EnsureRolesExist(roleMgr);
            
            CreateAdminUser(userMgr, seedPassword);
            CreateSellerUser(userMgr, seedPassword);
            CreateBuyerUsers(userMgr, seedPassword);
        }
    }

    private static void EnsureRolesExist(RoleManager<IdentityRole> roleMgr)
    {
        foreach (var roleName in AppRoles.AllRoles)
        {
            if (!roleMgr.RoleExistsAsync(roleName).Result)
            {
                var result = roleMgr.CreateAsync(new IdentityRole(roleName)).Result;
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role {roleName}: {result.Errors.First().Description}");
                }
                Log.Debug("{Role} role created", roleName);
            }
        }
    }

    private static void CreateAdminUser(UserManager<ApplicationUser> userMgr, string seedPassword)
    {
        var admin = userMgr.FindByNameAsync("admin").Result;
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@auction.com",
                EmailConfirmed = true,
                FullName = "System Administrator",
            };
            var result = userMgr.CreateAsync(admin, seedPassword).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddToRoleAsync(admin, AppRoles.Admin).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(admin, new Claim[]
            {
                new(ClaimTypes.Name, "System Administrator"),
                new(ClaimTypes.Role, AppRoles.Admin),
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("admin user created");
        }
        else
        {
            Log.Debug("admin user already exists");
        }
    }

    private static void CreateSellerUser(UserManager<ApplicationUser> userMgr, string seedPassword)
    {
        var seller = userMgr.FindByNameAsync("seller").Result;
        if (seller == null)
        {
            seller = new ApplicationUser
            {
                UserName = "seller",
                Email = "seller@auction.com",
                EmailConfirmed = true,
                FullName = "Demo Seller",
            };
            var result = userMgr.CreateAsync(seller, seedPassword).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddToRoleAsync(seller, AppRoles.Seller).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(seller, new Claim[]
            {
                new(ClaimTypes.Name, "Demo Seller"),
                new(ClaimTypes.Role, AppRoles.Seller),
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("seller user created");
        }
        else
        {
            Log.Debug("seller user already exists");
        }
    }

    private static void CreateBuyerUsers(UserManager<ApplicationUser> userMgr, string seedPassword)
    {
        var alice = userMgr.FindByNameAsync("alice").Result;
        if (alice == null)
        {
            alice = new ApplicationUser
            {
                UserName = "alice",
                Email = "alice@example.com",
                EmailConfirmed = true,
                FullName = "Alice Smith",
            };
            var result = userMgr.CreateAsync(alice, seedPassword).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddToRoleAsync(alice, AppRoles.User).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(alice, new Claim[]
            {
                new(ClaimTypes.Name, "Alice Smith"),
                new(ClaimTypes.Role, AppRoles.User),
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("alice user created");
        }
        else
        {
            Log.Debug("alice user already exists");
        }

        var bob = userMgr.FindByNameAsync("bob").Result;
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "bob@example.com",
                EmailConfirmed = true,
                FullName = "Bob Johnson",
            };
            var result = userMgr.CreateAsync(bob, seedPassword).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddToRoleAsync(bob, AppRoles.User).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(bob, new Claim[]
            {
                new(ClaimTypes.Name, "Bob Johnson"),
                new(ClaimTypes.Role, AppRoles.User),
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("bob user created");
        }
        else
        {
            Log.Debug("bob user already exists");
        }
    }
}
