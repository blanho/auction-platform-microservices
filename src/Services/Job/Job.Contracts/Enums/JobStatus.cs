namespace JobService.Contracts.Enums;

public enum JobStatus
{
    Initializing = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    CompletedWithErrors = 4,
    Failed = 5,
    Cancelled = 6
}
