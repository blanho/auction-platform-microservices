namespace Common.Audit.Configuration;
public class AuditOptions
{
    public const string SectionName = "Audit";
    public string ServiceName { get; set; } = "UnknownService";
    public bool Enabled { get; set; } = true;
    public HashSet<string> ExcludedProperties { get; set; } = ["Password", "PasswordHash", "Token", "Secret"];
    public HashSet<string> ExcludedEntities { get; set; } = [];
}
