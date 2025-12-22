namespace Common.Storage.Enums;

public enum StorageType
{
    Local,
    AzureBlob,
    S3
}


public enum FileStatus
{
    Temporary,
    Permanent,
    Deleted
}
