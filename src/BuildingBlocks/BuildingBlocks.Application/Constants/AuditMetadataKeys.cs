namespace BuildingBlocks.Application.Constants;

public static class AuditMetadataKeys
{
    public const string Action = "Action";
    public const string Amount = "Amount";
    public const string ReferenceId = "ReferenceId";
    public const string ReferenceType = "ReferenceType";
    public const string NewBalance = "NewBalance";
    public const string NewHeldAmount = "NewHeldAmount";
    public const string BuyerId = "BuyerId";
    public const string BuyerUsername = "BuyerUsername";
    public const string Price = "Price";
    public const string Type = "Type";
    public const string CorrelationId = "CorrelationId";
    public const string TotalItems = "TotalItems";
    public const string Reason = "Reason";
    public const string WasHighestBid = "WasHighestBid";
    public const string PreviousMaxAmount = "PreviousMaxAmount";
    public const string NewMaxAmount = "NewMaxAmount";
    public const string PreviousStatus = "PreviousStatus";
    public const string FileName = "FileName";
    public const string FileSize = "FileSize";
    public const string Provider = "Provider";
    public const string PresignedUpload = "PresignedUpload";
    public const string BatchUpload = "BatchUpload";
    public const string TrackingNumber = "TrackingNumber";
    public const string ShippingCarrier = "ShippingCarrier";
    public const string PaymentMethod = "PaymentMethod";
    public const string TransactionId = "TransactionId";
    public const string PreviousRoles = "PreviousRoles";
    public const string NewRoles = "NewRoles";

    /// <summary>
    /// Lowercase variants produce camelCase JSON keys in audit payloads,
    /// as required by the Identity service audit consumer.
    /// Do not remove without updating all Identity service callsites.
    /// </summary>
    public const string ActionLower = "action";
    public const string ReasonLower = "reason";
}
