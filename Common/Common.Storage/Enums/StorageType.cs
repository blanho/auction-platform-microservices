namespace Common.Storage.Enums;

public enum StorageType
{
    Local,
    AzureBlob,
    S3,
    Cloudinary
}


public enum FileStatus
{
    Temporary,
    Permanent,
    Deleted
}
