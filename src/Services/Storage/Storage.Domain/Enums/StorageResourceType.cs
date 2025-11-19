namespace Storage.Domain.Enums;

public enum StorageResourceType
{

    General = 0,

    AuctionImage = 1,

    AuctionDocument = 2,

    AuctionVideo = 3,

    AuctionGallery = 4,

    UserAvatar = 10,

    UserIdentityDocument = 11,

    UserVerificationPhoto = 12,

    UserSignature = 13,

    BidAttachment = 20,

    BidProofDocument = 21,

    PaymentReceipt = 30,

    InvoiceDocument = 31,

    RefundEvidence = 32,

    BankTransferProof = 33,

    OrganizationLogo = 40,

    OrganizationBanner = 41,

    OrganizationDocument = 42,

    EmailAttachment = 50,

    ChatAttachment = 51,

    ImportFile = 90,

    ExportFile = 91,

    ReportOutput = 92,

    BackupFile = 93,

    EmailTemplateAsset = 100,

    NotificationAttachment = 101
}
