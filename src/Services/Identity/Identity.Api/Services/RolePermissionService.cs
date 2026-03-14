using BuildingBlocks.Web.Authorization;
using Identity.Api.Data;
using Identity.Api.Interfaces;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly ApplicationDbContext _context;

    public RolePermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.AppRoles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();
        var permissionStrings = await _context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => roleIds.Contains(p.RoleId) && p.IsEnabled)
            .ToListAsync(cancellationToken);

        var permissionsByRole = permissionStrings
            .GroupBy(p => p.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.PermissionCode).ToList());

        return roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystemRole,
            permissionsByRole.GetValueOrDefault(r.Id, [])
        )).ToList();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _context.AppRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
            return null;

        var permissions = await _context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == roleId && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);

        return new RoleDto(role.Id, role.Name, role.Description, role.IsSystemRole, permissions);
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await _context.AppRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);

        if (role is null)
            return null;

        var permissions = await _context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == role.Id && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);

        return new RoleDto(role.Id, role.Name, role.Description, role.IsSystemRole, permissions);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => p.RoleId == roleId && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<HashSet<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        var roleNamesList = roleNames.ToList();

        var roleIds = await _context.AppRoles
            .AsNoTracking()
            .Where(r => roleNamesList.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return RolePermissions.GetPermissionsForRoles(roleNamesList);
        }

        var permissions = await _context.RolePermissionStrings
            .AsNoTracking()
            .Where(p => roleIds.Contains(p.RoleId) && p.IsEnabled)
            .Select(p => p.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (permissions.Count == 0)
        {
            return RolePermissions.GetPermissionsForRoles(roleNamesList);
        }

        return [..permissions];
    }

    public Task<IReadOnlyList<PermissionDefinition>> GetAllPermissionDefinitionsAsync()
    {
        var definitions = new List<PermissionDefinition>
        {
            new(Perm.AuctionView, "Auctions", "View Auctions", "View auction listings"),
            new(Perm.AuctionCreate, "Auctions", "Create Auctions", "Create new auction listings"),
            new(Perm.AuctionEdit, "Auctions", "Edit Auctions", "Edit own auction listings"),
            new(Perm.AuctionDelete, "Auctions", "Delete Auctions", "Delete auction listings"),
            new(Perm.AuctionModerate, "Auctions", "Moderate Auctions", "Moderate and approve auction listings"),
            new(Perm.AuctionExport, "Auctions", "Export Auctions", "Export auction data"),
            new(Perm.AuctionImport, "Auctions", "Import Auctions", "Import auction data"),
            new(Perm.CategoryManage, "Auctions", "Manage Categories", "Create, edit, and delete categories"),
            new(Perm.BrandManage, "Auctions", "Manage Brands", "Create, edit, and delete brands"),

            new(Perm.BidView, "Bids", "View Bids", "View bids on auctions"),
            new(Perm.BidPlace, "Bids", "Place Bids", "Place bids on auctions"),

            new(Perm.UserView, "Users", "View Users", "View user accounts"),
            new(Perm.UserCreate, "Users", "Create Users", "Create new user accounts"),
            new(Perm.UserEdit, "Users", "Edit Users", "Edit user accounts"),
            new(Perm.UserDelete, "Users", "Delete Users", "Delete user accounts"),
            new(Perm.UserBan, "Users", "Ban Users", "Ban/suspend user accounts"),
            new(Perm.UserManageRoles, "Users", "Manage User Roles", "Assign and remove roles from users"),

            new(Perm.OrderView, "Orders", "View All Orders", "View all orders"),
            new(Perm.OrderViewOwn, "Orders", "View Own Orders", "View own orders"),
            new(Perm.OrderCreate, "Orders", "Create Orders", "Create new orders"),
            new(Perm.OrderCancel, "Orders", "Cancel Orders", "Cancel orders"),
            new(Perm.OrderRefund, "Orders", "Refund Orders", "Process order refunds"),

            new(Perm.PaymentView, "Payments", "View Payments", "View payment transactions"),
            new(Perm.PaymentProcess, "Payments", "Process Payments", "Process payment transactions"),
            new(Perm.PaymentRefund, "Payments", "Refund Payments", "Refund payment transactions"),

            new(Perm.WalletView, "Wallets", "View All Wallets", "View all user wallets"),
            new(Perm.WalletViewOwn, "Wallets", "View Own Wallet", "View own wallet"),
            new(Perm.WalletDeposit, "Wallets", "Deposit to Wallet", "Deposit funds to wallet"),
            new(Perm.WalletWithdraw, "Wallets", "Withdraw from Wallet", "Withdraw funds from wallet"),

            new(Perm.AnalyticsViewPlatform, "Analytics", "View Platform Analytics", "View platform-wide analytics"),
            new(Perm.AnalyticsViewOwn, "Analytics", "View Own Analytics", "View personal analytics"),
            new(Perm.AnalyticsExport, "Analytics", "Export Analytics", "Export analytics data"),

            new(Perm.StorageView, "Storage", "View Files", "View files"),
            new(Perm.StorageUpload, "Storage", "Upload Files", "Upload files"),
            new(Perm.StorageDelete, "Storage", "Delete Files", "Delete files"),

            new(Perm.NotificationView, "Notifications", "View Notifications", "View notifications"),
            new(Perm.NotificationSend, "Notifications", "Send Notifications", "Send notifications to users"),
            new(Perm.NotificationManageTemplates, "Notifications", "Manage Templates", "Manage notification templates"),

            new(Perm.ReviewView, "Reviews", "View Reviews", "View product reviews"),
            new(Perm.ReviewCreate, "Reviews", "Create Reviews", "Create product reviews"),
            new(Perm.ReviewModerate, "Reviews", "Moderate Reviews", "Moderate and manage reviews"),

            new(Perm.AuditView, "Audit", "View Audit Logs", "View audit log entries"),
            new(Perm.AuditExport, "Audit", "Export Audit Logs", "Export audit log data"),

            new(Perm.ReportView, "Reports", "View Reports", "View reports"),
            new(Perm.ReportCreate, "Reports", "Create Reports", "Create reports"),
            new(Perm.ReportManage, "Reports", "Manage Reports", "Manage and process reports"),
        };

        return Task.FromResult<IReadOnlyList<PermissionDefinition>>(definitions);
    }

    public async Task<bool> GrantPermissionAsync(Guid roleId, string permission, CancellationToken cancellationToken = default)
    {
        var role = await _context.AppRoles.FindAsync([roleId], cancellationToken);
        if (role is null)
            return false;

        var existing = await _context.RolePermissionStrings
            .FirstOrDefaultAsync(p => p.RoleId == roleId && p.PermissionCode == permission, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsEnabled)
                return true;

            existing.IsEnabled = true;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            _context.RolePermissionStrings.Add(new RolePermissionString
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionCode = permission,
                IsEnabled = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RevokePermissionAsync(Guid roleId, string permission, CancellationToken cancellationToken = default)
    {
        var existing = await _context.RolePermissionStrings
            .FirstOrDefaultAsync(p => p.RoleId == roleId && p.PermissionCode == permission, cancellationToken);

        if (existing is null)
            return true;

        existing.IsEnabled = false;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetPermissionsAsync(Guid roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var role = await _context.AppRoles.FindAsync([roleId], cancellationToken);
        if (role is null)
            return false;

        var permissionSet = permissions.ToHashSet();

        var existing = await _context.RolePermissionStrings
            .Where(p => p.RoleId == roleId)
            .ToListAsync(cancellationToken);

        foreach (var perm in existing)
        {
            perm.IsEnabled = permissionSet.Contains(perm.PermissionCode);
            perm.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var existingCodes = existing.Select(p => p.PermissionCode).ToHashSet();
        var newPermissions = permissionSet.Except(existingCodes);

        foreach (var code in newPermissions)
        {
            _context.RolePermissionStrings.Add(new RolePermissionString
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionCode = code,
                IsEnabled = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
