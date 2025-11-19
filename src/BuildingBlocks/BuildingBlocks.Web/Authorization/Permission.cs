namespace BuildingBlocks.Web.Authorization;

public static class Perm
{

    public const string AuctionView = "auction:view";
    public const string AuctionCreate = "auction:create";
    public const string AuctionEdit = "auction:edit";
    public const string AuctionDelete = "auction:delete";
    public const string AuctionModerate = "auction:moderate";
    public const string CategoryManage = "category:manage";
    public const string BrandManage = "brand:manage";

    public const string BidView = "bid:view";
    public const string BidPlace = "bid:place";

    public const string UserView = "user:view";
    public const string UserCreate = "user:create";
    public const string UserEdit = "user:edit";
    public const string UserDelete = "user:delete";
    public const string UserBan = "user:ban";
    public const string UserManageRoles = "user:manage-roles";

    public const string OrderView = "order:view";
    public const string OrderViewOwn = "order:view-own";
    public const string OrderCreate = "order:create";
    public const string OrderCancel = "order:cancel";
    public const string OrderRefund = "order:refund";

    public const string PaymentView = "payment:view";
    public const string PaymentProcess = "payment:process";
    public const string PaymentRefund = "payment:refund";

    public const string WalletView = "wallet:view";
    public const string WalletViewOwn = "wallet:view-own";
    public const string WalletDeposit = "wallet:deposit";
    public const string WalletWithdraw = "wallet:withdraw";

    public const string AnalyticsViewPlatform = "analytics:view-platform";
    public const string AnalyticsViewOwn = "analytics:view-own";
    public const string AnalyticsExport = "analytics:export";

    public const string StorageView = "storage:view";
    public const string StorageUpload = "storage:upload";
    public const string StorageDelete = "storage:delete";

    public const string NotificationView = "notification:view";
    public const string NotificationSend = "notification:send";
    public const string NotificationManageTemplates = "notification:manage-templates";

    public const string ReviewView = "review:view";
    public const string ReviewCreate = "review:create";
    public const string ReviewModerate = "review:moderate";

    public const string AuditView = "audit:view";
    public const string AuditExport = "audit:export";

    public const string ReportView = "report:view";
    public const string ReportCreate = "report:create";
    public const string ReportManage = "report:manage";
}

public static class Permissions
{
    public static class Auctions
    {
        public const string View = Perm.AuctionView;
        public const string Create = Perm.AuctionCreate;
        public const string Edit = Perm.AuctionEdit;
        public const string Delete = Perm.AuctionDelete;
        public const string Moderate = Perm.AuctionModerate;
        public const string ManageCategories = Perm.CategoryManage;
        public const string ManageBrands = Perm.BrandManage;
    }

    public static class Bids
    {
        public const string View = Perm.BidView;
        public const string Place = Perm.BidPlace;
        public const string PlaceAuto = Perm.BidPlace;
        public const string ViewOwn = Perm.BidView;
    }

    public static class Orders
    {
        public const string View = Perm.OrderView;
        public const string ViewOwn = Perm.OrderViewOwn;
        public const string Create = Perm.OrderCreate;
        public const string Cancel = Perm.OrderCancel;
        public const string Refund = Perm.OrderRefund;
    }

    public static class Payments
    {
        public const string View = Perm.PaymentView;
        public const string Process = Perm.PaymentProcess;
        public const string Refund = Perm.PaymentRefund;
    }

    public static class Wallets
    {
        public const string View = Perm.WalletView;
        public const string ViewOwn = Perm.WalletViewOwn;
        public const string Deposit = Perm.WalletDeposit;
        public const string Withdraw = Perm.WalletWithdraw;
    }

    public static class Users
    {
        public const string View = Perm.UserView;
        public const string Create = Perm.UserCreate;
        public const string Edit = Perm.UserEdit;
        public const string Delete = Perm.UserDelete;
        public const string ManageRoles = Perm.UserManageRoles;
        public const string Ban = Perm.UserBan;
        public const string ManageSettings = Perm.UserEdit;
    }

    public static class Analytics
    {
        public const string ViewPlatform = Perm.AnalyticsViewPlatform;
        public const string ViewSeller = Perm.AnalyticsViewOwn;
        public const string ViewOwn = Perm.AnalyticsViewOwn;
        public const string ViewAuctions = Perm.AnalyticsViewPlatform;
        public const string ViewUsers = Perm.AnalyticsViewPlatform;
        public const string ViewRevenue = Perm.AnalyticsViewPlatform;
        public const string ViewBids = Perm.AnalyticsViewPlatform;
        public const string ViewCategories = Perm.AnalyticsViewPlatform;
        public const string Export = Perm.AnalyticsExport;
    }

    public static class Storage
    {
        public const string Upload = Perm.StorageUpload;
        public const string Delete = Perm.StorageDelete;
        public const string View = Perm.StorageView;
    }

    public static class Notifications
    {
        public const string View = Perm.NotificationView;
        public const string Send = Perm.NotificationSend;
        public const string ManageTemplates = Perm.NotificationManageTemplates;
    }

    public static class Reviews
    {
        public const string View = Perm.ReviewView;
        public const string Create = Perm.ReviewCreate;
        public const string Edit = Perm.ReviewCreate;
        public const string Delete = Perm.ReviewModerate;
        public const string Moderate = Perm.ReviewModerate;
    }

    public static class AuditLogs
    {
        public const string View = Perm.AuditView;
        public const string Export = Perm.AuditExport;
    }

    public static class Reports
    {
        public const string View = Perm.ReportView;
        public const string Create = Perm.ReportCreate;
        public const string UpdateStatus = Perm.ReportManage;
        public const string UpdatePriority = Perm.ReportManage;
        public const string Assign = Perm.ReportManage;
        public const string Resolve = Perm.ReportManage;
        public const string Delete = Perm.ReportManage;
        public const string Manage = Perm.ReportManage;
    }
}
