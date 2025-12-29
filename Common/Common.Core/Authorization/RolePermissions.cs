namespace Common.Core.Authorization;

public static class RolePermissions
{
    private static readonly Dictionary<string, HashSet<string>> RolePermissionMap = new()
    {
        [AppRoles.Admin] = new HashSet<string>
        {
            // Auctions - Full Access
            Permissions.Auctions.View,
            Permissions.Auctions.Create,
            Permissions.Auctions.Edit,
            Permissions.Auctions.Delete,
            Permissions.Auctions.Moderate,
            Permissions.Auctions.ManageCategories,
            Permissions.Auctions.ManageBrands,

            // Bids - Full Access
            Permissions.Bids.View,
            Permissions.Bids.Place,
            Permissions.Bids.PlaceAuto,
            Permissions.Bids.ViewOwn,

            // Orders - Full Access
            Permissions.Orders.View,
            Permissions.Orders.ViewOwn,
            Permissions.Orders.Create,
            Permissions.Orders.Cancel,
            Permissions.Orders.Refund,

            // Payments - Full Access
            Permissions.Payments.View,
            Permissions.Payments.Process,
            Permissions.Payments.Refund,

            // Wallets - Full Access
            Permissions.Wallets.View,
            Permissions.Wallets.ViewOwn,
            Permissions.Wallets.Deposit,
            Permissions.Wallets.Withdraw,

            // Users - Full Access
            Permissions.Users.View,
            Permissions.Users.Create,
            Permissions.Users.Edit,
            Permissions.Users.Delete,
            Permissions.Users.ManageRoles,
            Permissions.Users.Ban,

            // Analytics - Full Access
            Permissions.Analytics.ViewPlatform,
            Permissions.Analytics.ViewSeller,
            Permissions.Analytics.ViewOwn,
            Permissions.Analytics.Export,

            // Storage - Full Access
            Permissions.Storage.Upload,
            Permissions.Storage.Delete,
            Permissions.Storage.View,

            // Notifications - Full Access
            Permissions.Notifications.View,
            Permissions.Notifications.Send,
            Permissions.Notifications.ManageTemplates,

            // Reviews - Full Access
            Permissions.Reviews.View,
            Permissions.Reviews.Create,
            Permissions.Reviews.Edit,
            Permissions.Reviews.Delete,
            Permissions.Reviews.Moderate,

            // Audit Logs - Full Access
            Permissions.AuditLogs.View,
            Permissions.AuditLogs.Export,

            // Reports - Full Access
            Permissions.Reports.View,
            Permissions.Reports.Create,
            Permissions.Reports.Manage,

            // Settings - Full Access
            Permissions.Users.ManageSettings,
        },

        [AppRoles.Seller] = new HashSet<string>
        {
            // Auctions - Create/Edit own
            Permissions.Auctions.View,
            Permissions.Auctions.Create,
            Permissions.Auctions.Edit,

            // Bids - Full bidding access
            Permissions.Bids.View,
            Permissions.Bids.Place,
            Permissions.Bids.PlaceAuto,
            Permissions.Bids.ViewOwn,

            // Orders - View own, Create
            Permissions.Orders.ViewOwn,
            Permissions.Orders.Create,

            // Wallets - Own wallet management
            Permissions.Wallets.ViewOwn,
            Permissions.Wallets.Deposit,
            Permissions.Wallets.Withdraw,

            // Analytics - Seller analytics
            Permissions.Analytics.ViewSeller,
            Permissions.Analytics.ViewOwn,

            // Storage - Upload for listings
            Permissions.Storage.Upload,
            Permissions.Storage.View,

            // Notifications - View own
            Permissions.Notifications.View,

            // Reviews - Create/Edit own
            Permissions.Reviews.View,
            Permissions.Reviews.Create,
            Permissions.Reviews.Edit,

            // Reports - Create own
            Permissions.Reports.Create,
        },

        [AppRoles.User] = new HashSet<string>
        {
            // Auctions - View only
            Permissions.Auctions.View,

            // Bids - Full bidding access
            Permissions.Bids.View,
            Permissions.Bids.Place,
            Permissions.Bids.PlaceAuto,
            Permissions.Bids.ViewOwn,

            // Orders - View own, Create (buy)
            Permissions.Orders.ViewOwn,
            Permissions.Orders.Create,

            // Wallets - Own wallet
            Permissions.Wallets.ViewOwn,
            Permissions.Wallets.Deposit,

            // Analytics - View own only
            Permissions.Analytics.ViewOwn,

            // Storage - View only
            Permissions.Storage.View,

            // Notifications - View own
            Permissions.Notifications.View,

            // Reviews - Create/Edit own
            Permissions.Reviews.View,
            Permissions.Reviews.Create,
            Permissions.Reviews.Edit,

            // Reports - Create own
            Permissions.Reports.Create,
        }
    };

    public static HashSet<string> GetPermissionsForRole(string role)
    {
        return RolePermissionMap.TryGetValue(role, out var permissions)
            ? permissions
            : new HashSet<string>();
    }

    public static HashSet<string> GetPermissionsForRoles(IEnumerable<string> roles)
    {
        var allPermissions = new HashSet<string>();

        foreach (var role in roles)
        {
            if (RolePermissionMap.TryGetValue(role, out var permissions))
            {
                allPermissions.UnionWith(permissions);
            }
        }

        return allPermissions;
    }

    public static bool RoleHasPermission(string role, string permission)
    {
        return RolePermissionMap.TryGetValue(role, out var permissions)
            && permissions.Contains(permission);
    }

    public static IReadOnlyList<string> GetAllPermissions()
    {
        return RolePermissionMap.Values
            .SelectMany(p => p)
            .Distinct()
            .OrderBy(p => p)
            .ToList();
    }
}
