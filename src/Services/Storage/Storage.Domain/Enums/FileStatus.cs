namespace Storage.Domain.Enums;

public enum FileStatus
{

    Pending = 0,

    InTemp = 1,

    Scanning = 2,

    Scanned = 3,

    InMedia = 4,

    Processing = 5,

    Infected = 6,

    Expired = 7,

    Removed = 8,

    Uploaded = 10,

    Confirmed = 11,

    Failed = 12
}
