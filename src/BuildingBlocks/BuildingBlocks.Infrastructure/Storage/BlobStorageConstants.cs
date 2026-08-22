namespace BuildingBlocks.Infrastructure.Storage;

internal static class BlobStorageConstants
{
    internal static class MetadataKeys
    {
        public const string OriginalFileName = "OriginalFileName";
        public const string OwnerId = "OwnerId";
    }

    internal static class Headers
    {
        public const string BlobType = "x-ms-blob-type";
        public const string ContentType = "Content-Type";
    }

    public const string BlockBlob = "BlockBlob";
    public const string BlobResource = "b";
}
