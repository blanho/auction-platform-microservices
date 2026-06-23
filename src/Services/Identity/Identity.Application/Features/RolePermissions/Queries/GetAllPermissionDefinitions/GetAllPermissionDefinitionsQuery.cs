namespace Identity.Application.Features.RolePermissions.Queries.GetAllPermissionDefinitions;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetAllPermissionDefinitionsQuery() : IQuery<IReadOnlyList<Identity.Application.Interfaces.PermissionDefinition>>;

public class GetAllPermissionDefinitionsQueryHandler(
    ILogger<GetAllPermissionDefinitionsQueryHandler> logger) : IQueryHandler<GetAllPermissionDefinitionsQuery, IReadOnlyList<Identity.Application.Interfaces.PermissionDefinition>>
{
    public async Task<Result<IReadOnlyList<Identity.Application.Interfaces.PermissionDefinition>>> Handle(GetAllPermissionDefinitionsQuery query, CancellationToken cancellationToken)
    {
        var definitions = new List<Identity.Application.Interfaces.PermissionDefinition>
        {
            new(BuildingBlocks.Web.Authorization.Perm.AuctionView, "Auctions", "View Auctions", "View auction listings"),
            new(BuildingBlocks.Web.Authorization.Perm.AuctionCreate, "Auctions", "Create Auctions", "Create new auction listings"),
            new(BuildingBlocks.Web.Authorization.Perm.AuctionEdit, "Auctions", "Edit Auctions", "Edit own auction listings"),
            new(BuildingBlocks.Web.Authorization.Perm.AuctionDelete, "Auctions", "Delete Auctions", "Delete auction listings"),
            new(BuildingBlocks.Web.Authorization.Perm.AuctionModerate, "Auctions", "Moderate Auctions", "Moderate and approve auction listings"),
            new(BuildingBlocks.Web.Authorization.Perm.AuctionExport, "Auctions", "Export Auctions", "Export auction data"),
            new(BuildingBlocks.Web.Authorization.Perm.AuctionImport, "Auctions", "Import Auctions", "Import auction data"),
            new(BuildingBlocks.Web.Authorization.Perm.CategoryManage, "Auctions", "Manage Categories", "Create, edit, and delete categories"),
            new(BuildingBlocks.Web.Authorization.Perm.BrandManage, "Auctions", "Manage Brands", "Create, edit, and delete brands"),

            new(BuildingBlocks.Web.Authorization.Perm.BidView, "Bids", "View Bids", "View bids on auctions"),
            new(BuildingBlocks.Web.Authorization.Perm.BidPlace, "Bids", "Place Bids", "Place bids on auctions"),

            new(BuildingBlocks.Web.Authorization.Perm.UserView, "Users", "View Users", "View user accounts"),
            new(BuildingBlocks.Web.Authorization.Perm.UserCreate, "Users", "Create Users", "Create new user accounts"),
            new(BuildingBlocks.Web.Authorization.Perm.UserEdit, "Users", "Edit Users", "Edit user accounts"),
            new(BuildingBlocks.Web.Authorization.Perm.UserDelete, "Users", "Delete Users", "Delete user accounts"),
            new(BuildingBlocks.Web.Authorization.Perm.UserBan, "Users", "Ban Users", "Ban/suspend user accounts"),
            new(BuildingBlocks.Web.Authorization.Perm.UserManageRoles, "Users", "Manage User Roles", "Assign and remove roles from users"),

            new(BuildingBlocks.Web.Authorization.Perm.OrderView, "Orders", "View All Orders", "View all orders"),
            new(BuildingBlocks.Web.Authorization.Perm.OrderViewOwn, "Orders", "View Own Orders", "View own orders"),
            new(BuildingBlocks.Web.Authorization.Perm.OrderCreate, "Orders", "Create Orders", "Create new orders"),
            new(BuildingBlocks.Web.Authorization.Perm.OrderCancel, "Orders", "Cancel Orders", "Cancel orders"),
            new(BuildingBlocks.Web.Authorization.Perm.OrderRefund, "Orders", "Refund Orders", "Process order refunds"),

            new(BuildingBlocks.Web.Authorization.Perm.PaymentView, "Payments", "View Payments", "View payment transactions"),
            new(BuildingBlocks.Web.Authorization.Perm.PaymentProcess, "Payments", "Process Payments", "Process payment transactions"),
            new(BuildingBlocks.Web.Authorization.Perm.PaymentRefund, "Payments", "Refund Payments", "Refund payment transactions"),

            new(BuildingBlocks.Web.Authorization.Perm.WalletView, "Wallets", "View All Wallets", "View all user wallets"),
            new(BuildingBlocks.Web.Authorization.Perm.WalletViewOwn, "Wallets", "View Own Wallet", "View own wallet"),
            new(BuildingBlocks.Web.Authorization.Perm.WalletDeposit, "Wallets", "Deposit to Wallet", "Deposit funds to wallet"),
            new(BuildingBlocks.Web.Authorization.Perm.WalletWithdraw, "Wallets", "Withdraw from Wallet", "Withdraw funds from wallet"),

            new(BuildingBlocks.Web.Authorization.Perm.AnalyticsViewPlatform, "Analytics", "View Platform Analytics", "View platform-wide analytics"),
            new(BuildingBlocks.Web.Authorization.Perm.AnalyticsViewOwn, "Analytics", "View Own Analytics", "View personal analytics"),
            new(BuildingBlocks.Web.Authorization.Perm.AnalyticsExport, "Analytics", "Export Analytics", "Export analytics data"),

            new(BuildingBlocks.Web.Authorization.Perm.StorageView, "Storage", "View Files", "View files"),
            new(BuildingBlocks.Web.Authorization.Perm.StorageUpload, "Storage", "Upload Files", "Upload files"),
            new(BuildingBlocks.Web.Authorization.Perm.StorageDelete, "Storage", "Delete Files", "Delete files"),

            new(BuildingBlocks.Web.Authorization.Perm.NotificationView, "Notifications", "View Notifications", "View notifications"),
            new(BuildingBlocks.Web.Authorization.Perm.NotificationSend, "Notifications", "Send Notifications", "Send notifications to users"),
            new(BuildingBlocks.Web.Authorization.Perm.NotificationManageTemplates, "Notifications", "Manage Templates", "Manage notification templates"),

            new(BuildingBlocks.Web.Authorization.Perm.ReviewView, "Reviews", "View Reviews", "View product reviews"),
            new(BuildingBlocks.Web.Authorization.Perm.ReviewCreate, "Reviews", "Create Reviews", "Create product reviews"),
            new(BuildingBlocks.Web.Authorization.Perm.ReviewModerate, "Reviews", "Moderate Reviews", "Moderate and manage reviews"),

            new(BuildingBlocks.Web.Authorization.Perm.AuditView, "Audit", "View Audit Logs", "View audit log entries"),
            new(BuildingBlocks.Web.Authorization.Perm.AuditExport, "Audit", "Export Audit Logs", "Export audit log data"),

            new(BuildingBlocks.Web.Authorization.Perm.ReportView, "Reports", "View Reports", "View reports"),
            new(BuildingBlocks.Web.Authorization.Perm.ReportCreate, "Reports", "Create Reports", "Create reports"),
            new(BuildingBlocks.Web.Authorization.Perm.ReportManage, "Reports", "Manage Reports", "Manage and process reports"),
        };

        return await Task.FromResult(Result.Success<IReadOnlyList<Identity.Application.Interfaces.PermissionDefinition>>(definitions));
    }
}
