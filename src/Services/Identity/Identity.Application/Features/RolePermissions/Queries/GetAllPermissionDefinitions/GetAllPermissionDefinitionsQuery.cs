namespace Identity.Application.Features.RolePermissions.Queries.GetAllPermissionDefinitions;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;

public record GetAllPermissionDefinitionsQuery() : IQuery<IReadOnlyList<PermissionDefinition>>;

public class GetAllPermissionDefinitionsQueryHandler : IQueryHandler<GetAllPermissionDefinitionsQuery, IReadOnlyList<PermissionDefinition>>
{
    public async Task<Result<IReadOnlyList<PermissionDefinition>>> Handle(GetAllPermissionDefinitionsQuery query, CancellationToken cancellationToken)
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
            new(Perm.OrderRefund, "Orders", "Refund Orders", "Process order refunds"),

            new(Perm.PaymentView, "Payments", "View Payments", "View payment transactions"),
            new(Perm.PaymentProcess, "Payments", "Process Payments", "Process payment transactions"),
            new(Perm.PaymentRefund, "Payments", "Refund Payments", "Refund payment transactions"),

            new(Perm.WalletView, "Wallets", "View All Wallets", "View all user wallets"),
            new(Perm.WalletViewOwn, "Wallets", "View Own Wallet", "View own wallet"),

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

        return await Task.FromResult(Result.Success<IReadOnlyList<PermissionDefinition>>(definitions));
    }
}
