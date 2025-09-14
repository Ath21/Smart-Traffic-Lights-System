namespace LogData;

public class LogDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string Database { get; set; } = null!;
    public string AuditLogsCollection { get; set; } = "audit_logs";
    public string ErrorLogsCollection { get; set; } = "error_logs";
    public string FailoverLogsCollection { get; set; } = "failover_logs";
}
