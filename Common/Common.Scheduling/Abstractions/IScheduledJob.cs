using Quartz;

namespace Common.Scheduling.Abstractions;

/// <summary>
/// Base interface for scheduled jobs with common functionality
/// </summary>
public interface IScheduledJob : IJob
{
    /// <summary>
    /// Unique identifier for the job
    /// </summary>
    static abstract string JobId { get; }
    
    /// <summary>
    /// Human-readable description of the job
    /// </summary>
    static abstract string Description { get; }
}

/// <summary>
/// Configuration for a scheduled job
/// </summary>
public class JobScheduleConfig
{
    /// <summary>
    /// Cron expression for scheduling (e.g., "0 0/5 * * * ?" for every 5 minutes)
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;
    
    /// <summary>
    /// Simple interval in seconds (alternative to cron)
    /// </summary>
    public int? IntervalSeconds { get; set; }
    
    /// <summary>
    /// Whether the job is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Whether to run immediately on startup
    /// </summary>
    public bool RunOnStartup { get; set; } = false;
    
    /// <summary>
    /// Misfire instruction for the trigger
    /// </summary>
    public MisfireInstruction MisfirePolicy { get; set; } = MisfireInstruction.SmartPolicy;
}

public enum MisfireInstruction
{
    SmartPolicy,
    IgnoreMisfires,
    FireOnceNow,
    DoNothing
}
