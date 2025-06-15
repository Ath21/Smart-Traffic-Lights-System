using System;

namespace LogData;

public class LogDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string LogsCollectionName { get; set; } = null!;   
}

