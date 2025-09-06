namespace LogData;

public class LogDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string AuditLogsCollectionName { get; set; } = "audit_logs";
    public string ErrorLogsCollectionName { get; set; } = "error_logs";
}
