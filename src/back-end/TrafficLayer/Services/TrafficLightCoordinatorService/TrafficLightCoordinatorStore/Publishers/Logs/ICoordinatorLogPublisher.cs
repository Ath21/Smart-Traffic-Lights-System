using System;

namespace TrafficLightCoordinatorStore.Publishers.Logs;

public interface ICoordinatorLogPublisher
{
    Task PublishAuditAsync(
        string operation,
        string message,
        string domain = "[COORDINATOR]",
        string category = "system",
        Dictionary<string, object>? data = null);

    Task PublishErrorAsync(
        string operation,
        string message,
        Exception? ex = null,
        string domain = "[COORDINATOR]",
        string category = "system",
        Dictionary<string, object>? data = null);

    Task PublishFailoverAsync(
        string operation,
        string message,
        string domain = "[COORDINATOR]",
        string category = "system",
        Dictionary<string, object>? data = null);
}

