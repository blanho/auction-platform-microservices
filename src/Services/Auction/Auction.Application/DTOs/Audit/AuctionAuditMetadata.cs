using Auctions.Domain.Enums;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.DTOs.Audit;

public static class AuctionAuditMetadata
{
    private const string Activated = "Activated";
    private const string Deactivated = "Deactivated";
    private const string Cancelled = "Cancelled";
    private const string Extended = "Extended";
    private const string BuyNow = "BuyNow";
    private const string BulkActivated = "BulkActivated";
    private const string BulkDeactivated = "BulkDeactivated";

    public static Dictionary<string, object> ForActivation(Status previousStatus) =>
        ForStatusChange(Activated, previousStatus);

    public static Dictionary<string, object> ForDeactivation(Status previousStatus) =>
        ForStatusChange(Deactivated, previousStatus);

    public static Dictionary<string, object> ForCancellation(string? reason) =>
        new()
        {
            [AuditMetadataKeys.Action] = Cancelled,
            [AuditMetadataKeys.Reason] = reason ?? string.Empty
        };

    public static Dictionary<string, object> ForExtension(
        int extensionMinutes,
        DateTimeOffset previousEnd,
        DateTimeOffset newEnd) =>
        new()
        {
            [AuditMetadataKeys.Action] = Extended,
            [AuditMetadataKeys.ExtensionMinutes] = extensionMinutes,
            [AuditMetadataKeys.PreviousEnd] = previousEnd,
            [AuditMetadataKeys.NewEnd] = newEnd
        };

    public static Dictionary<string, object> ForBuyNow(
        Guid buyerId,
        string buyerUsername,
        decimal price) =>
        new()
        {
            [AuditMetadataKeys.Action] = BuyNow,
            [AuditMetadataKeys.BuyerId] = buyerId,
            [AuditMetadataKeys.BuyerUsername] = buyerUsername,
            [AuditMetadataKeys.Price] = price
        };

    public static Dictionary<string, object> ForBulkStatusChange(bool activate) =>
        new()
        {
            [AuditMetadataKeys.Action] = activate ? BulkActivated : BulkDeactivated
        };

    public static Dictionary<string, object> ForModifiedFields(IReadOnlyCollection<string> modifiedFields) =>
        new()
        {
            [AuditMetadataKeys.ModifiedFields] = modifiedFields
        };

    private static Dictionary<string, object> ForStatusChange(string action, Status previousStatus) =>
        new()
        {
            [AuditMetadataKeys.Action] = action,
            [AuditMetadataKeys.PreviousStatus] = previousStatus.ToString()
        };
}
