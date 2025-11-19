namespace BuildingBlocks.Domain.Authorization;

[Flags]
public enum Permission
{
    None             = 0,
    Read             = 1 << 0,
    Manage           = 1 << 1,
    ManagePermission = 1 << 2,
    Create           = 1 << 3,
    Update           = 1 << 4,
    Delete           = 1 << 5,
    All              = 63
}

public enum ResourceType
{

    Auction = 1,
    Bid = 2,
    Payment = 3,
    Order = 4,
    UserProfile = 5,
    Review = 6,
    File = 7,

    System = 100,
    Tenant = 101,
    Analytics = 102,
    AuditLog = 103,
    Report = 104,
    Notification = 105
}

public enum AccessScope
{

    None = 0,

    Owned = 1,

    Tenant = 2,

    Global = 3
}
