namespace Common.Core.Authorization;

public static class Permissions
{
    public static class Auctions
    {
        public const string View = "Permissions.Auctions.View";
        public const string Create = "Permissions.Auctions.Create";
        public const string Edit = "Permissions.Auctions.Edit";
        public const string Delete = "Permissions.Auctions.Delete";
        public const string Moderate = "Permissions.Auctions.Moderate";
        public const string ManageCategories = "Permissions.Auctions.ManageCategories";
        public const string ManageBrands = "Permissions.Auctions.ManageBrands";
    }

    public static class Bids
    {
        public const string View = "Permissions.Bids.View";
        public const string Place = "Permissions.Bids.Place";
        public const string PlaceAuto = "Permissions.Bids.PlaceAuto";
        public const string ViewOwn = "Permissions.Bids.ViewOwn";
    }

    public static class Orders
    {
        public const string View = "Permissions.Orders.View";
        public const string ViewOwn = "Permissions.Orders.ViewOwn";
        public const string Create = "Permissions.Orders.Create";
        public const string Cancel = "Permissions.Orders.Cancel";
        public const string Refund = "Permissions.Orders.Refund";
    }

    public static class Payments
    {
        public const string View = "Permissions.Payments.View";
        public const string Process = "Permissions.Payments.Process";
        public const string Refund = "Permissions.Payments.Refund";
    }

    public static class Wallets
    {
        public const string View = "Permissions.Wallets.View";
        public const string ViewOwn = "Permissions.Wallets.ViewOwn";
        public const string Deposit = "Permissions.Wallets.Deposit";
        public const string Withdraw = "Permissions.Wallets.Withdraw";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string ManageRoles = "Permissions.Users.ManageRoles";
        public const string Ban = "Permissions.Users.Ban";
        public const string ManageSettings = "Permissions.Users.ManageSettings";
    }

    public static class Analytics
    {
        public const string ViewPlatform = "Permissions.Analytics.ViewPlatform";
        public const string ViewSeller = "Permissions.Analytics.ViewSeller";
        public const string ViewOwn = "Permissions.Analytics.ViewOwn";
        public const string Export = "Permissions.Analytics.Export";
    }

    public static class Storage
    {
        public const string Upload = "Permissions.Storage.Upload";
        public const string Delete = "Permissions.Storage.Delete";
        public const string View = "Permissions.Storage.View";
    }

    public static class Notifications
    {
        public const string View = "Permissions.Notifications.View";
        public const string Send = "Permissions.Notifications.Send";
        public const string ManageTemplates = "Permissions.Notifications.ManageTemplates";
    }

    public static class Reviews
    {
        public const string View = "Permissions.Reviews.View";
        public const string Create = "Permissions.Reviews.Create";
        public const string Edit = "Permissions.Reviews.Edit";
        public const string Delete = "Permissions.Reviews.Delete";
        public const string Moderate = "Permissions.Reviews.Moderate";
    }

    public static class AuditLogs
    {
        public const string View = "Permissions.AuditLogs.View";
        public const string Export = "Permissions.AuditLogs.Export";
    }

    public static class Reports
    {
        public const string View = "Permissions.Reports.View";
        public const string Create = "Permissions.Reports.Create";
        public const string Manage = "Permissions.Reports.Manage";
    }
}
