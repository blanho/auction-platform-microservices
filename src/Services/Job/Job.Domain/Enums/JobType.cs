namespace Jobs.Domain.Enums;

public enum JobType
{
    UserImport = 0,
    AuctionSettlement = 1,
    SearchReindex = 2,
    PaymentCycle = 3,
    DataExport = 4,
    BulkNotification = 5,
    AnalyticsAggregation = 6,
    AuctionImport = 7,
    BulkAuctionUpdate = 8,
    BulkFileUpload = 9,
    ReportGeneration = 10,
    ImageProcessing = 11
}
