namespace Common.Core.Authorization;

public static class RolePermissions
{
    private static readonly Dictionary<string, HashSet<string>> RolePermissionMap = new()
    {
        [AppRoles.Admin] = new HashSet<string>
        {
            Permissions.Auctions.View,
            Permissions.Auctions.Create,
            Permissions.Auctions.Edit,
            Permissions.Auctions.Delete,
            Permissions.Auctions.Moderate,
            Permissions.Auctions.ManageCategories,
            Permissions.Auctions.ManageBrands,
            Permissions.Bids.View,
            Permissions.Bids.Place,
            Permissions.Bids.PlaceAuto,
            Permissions.Bids.ViewOwn,
            Permissions.Orders.View,
            Permissions.Orders.ViewOwn,
            Permissions.Orders.Create,
            Permissions.Orders.Cancel,
            Permissions.Orders.Refund,
            Permissions.Payments.View,
            Permissions.Payments.Process,
            Permissions.Payments.Refund,
            Permissions.Wallets.View,
            Permissions.Wallets.ViewOwn,
            Permissions.Wallets.Deposit,
            Permissions.Wallets.Withdraw,
            Permissions.Users.View,
            Permissions.Users.Create,
            Permissions.Users.Edit,
            Permissions.Users.Delete,
            Permissions.Users.ManageRoles,
            Permissions.Users.Ban,
            Permissions.Analytics.ViewPlatform,
            Permissions.Analytics.ViewSeller,
            Permissions.Analytics.ViewOwn,
            Permissions.Analytics.Export,
            Permissions.Storage.Upload,
            Permissions.Storage.Delete,
            Permissions.Storage.View,
            Permissions.Notifications.View,
            Permissions.Notifications.Send,
            Permissions.Notifications.ManageTemplates,
            Permissions.Reviews.View,
            Permissions.Reviews.Create,
            Permissions.Reviews.Edit,
            Permissions.Reviews.Delete,
            Permissions.Reviews.Moderate,
            Permissions.AuditLogs.View,
            Permissions.AuditLogs.Export,
            Permissions.Reports.View,
            Permissions.Reports.Create,
            Permissions.Reports.Manage,
            Permissions.Users.ManageSettings,
        },

        [AppRoles.Seller] = new HashSet<string>
        {
            Permissions.Auctions.View,
            Permissions.Auctions.Create,
            Permissions.Auctions.Edit,
            Permissions.Bids.View,
            Permissions.Bids.Place,
            Permissions.Bids.PlaceAuto,
            Permissions.Bids.ViewOwn,
            Permissions.Orders.ViewOwn,
            Permissions.Orders.Create,
            Permissions.Wallets.ViewOwn,
            Permissions.Wallets.Deposit,
            Permissions.Wallets.Withdraw,
            Permissions.Analytics.ViewSeller,
            Permissions.Analytics.ViewOwn,
            Permissions.Storage.Upload,
            Permissions.Storage.View,
            Permissions.Notifications.View,
            Permissions.Reviews.View,
            Permissions.Reviews.Create,
            Permissions.Reviews.Edit,
            Permissions.Reports.Create,
        },

        [AppRoles.User] = new HashSet<string>
        {
            Permissions.Auctions.View,
            Permissions.Bids.View,
            Permissions.Bids.Place,
            Permissions.Bids.PlaceAuto,
            Permissions.Bids.ViewOwn,
            Permissions.Orders.ViewOwn,
            Permissions.Orders.Create,
            Permissions.Wallets.ViewOwn,
            Permissions.Wallets.Deposit,
            Permissions.Analytics.ViewOwn,
            Permissions.Storage.View,
            Permissions.Notifications.View,
            Permissions.Reviews.View,
            Permissions.Reviews.Create,
            Permissions.Reviews.Edit,
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
