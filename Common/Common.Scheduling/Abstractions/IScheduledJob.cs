using Quartz;

namespace Common.Scheduling.Abstractions;

public interface IScheduledJob : IJob
{
    static abstract string JobId { get; }
    
    static abstract string Description { get; }
}

public class JobScheduleConfig
{
    public string CronExpression { get; set; } = string.Empty;
    
    public int? IntervalSeconds { get; set; }
    
    public bool Enabled { get; set; } = true;
    
    public bool RunOnStartup { get; set; } = false;
    
    public MisfireInstruction MisfirePolicy { get; set; } = MisfireInstruction.SmartPolicy;
}

public enum MisfireInstruction
{
    SmartPolicy,
    IgnoreMisfires,
    FireOnceNow,
    DoNothing
}
