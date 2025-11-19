namespace BuildingBlocks.Web.Authorization;

public static class RolePermissions
{
    private static readonly HashSet<string> UserPermissions =
    [
        Perm.AuctionView,
        Perm.BidView,
        Perm.BidPlace,
        Perm.OrderViewOwn,
        Perm.OrderCreate,
        Perm.WalletViewOwn,
        Perm.WalletDeposit,
        Perm.AnalyticsViewOwn,
        Perm.StorageView,
        Perm.NotificationView,
        Perm.ReviewView,
        Perm.ReviewCreate,
        Perm.ReportCreate,
    ];

    private static readonly HashSet<string> SellerPermissions =
    [

        ..UserPermissions,

        Perm.AuctionCreate,
        Perm.AuctionEdit,
        Perm.StorageUpload,
        Perm.WalletWithdraw,
    ];

    private static readonly HashSet<string> AdminPermissions =
    [

        ..SellerPermissions,

        Perm.AuctionDelete,
        Perm.AuctionModerate,
        Perm.CategoryManage,
        Perm.BrandManage,
        Perm.UserView,
        Perm.UserCreate,
        Perm.UserEdit,
        Perm.UserDelete,
        Perm.UserBan,
        Perm.UserManageRoles,
        Perm.OrderView,
        Perm.OrderCancel,
        Perm.OrderRefund,
        Perm.PaymentView,
        Perm.PaymentProcess,
        Perm.PaymentRefund,
        Perm.WalletView,
        Perm.AnalyticsViewPlatform,
        Perm.AnalyticsExport,
        Perm.StorageDelete,
        Perm.NotificationSend,
        Perm.NotificationManageTemplates,
        Perm.ReviewModerate,
        Perm.AuditView,
        Perm.AuditExport,
        Perm.ReportView,
        Perm.ReportManage,
    ];

    private static readonly Dictionary<string, HashSet<string>> RoleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [Roles.User] = UserPermissions,
        [Roles.Seller] = SellerPermissions,
        [Roles.Admin] = AdminPermissions,
    };

    public static bool HasPermission(string role, string permission)
        => RoleMap.TryGetValue(role, out var permissions) && permissions.Contains(permission);

    public static bool HasPermission(IEnumerable<string> roles, string permission)
        => roles.Any(role => HasPermission(role, permission));

    public static IReadOnlySet<string> GetPermissions(string role)
        => RoleMap.TryGetValue(role, out var permissions) ? permissions : new HashSet<string>();

    public static HashSet<string> GetPermissions(IEnumerable<string> roles)
    {
        var result = new HashSet<string>();
        foreach (var role in roles)
        {
            if (RoleMap.TryGetValue(role, out var permissions))
                result.UnionWith(permissions);
        }
        return result;
    }

    public static bool RoleHasPermission(string role, string permission) => HasPermission(role, permission);
    public static HashSet<string> GetPermissionsForRole(string role) => GetPermissions(role).ToHashSet();
    public static HashSet<string> GetPermissionsForRoles(IEnumerable<string> roles) => GetPermissions(roles);
}
