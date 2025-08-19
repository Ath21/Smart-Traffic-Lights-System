using System;

namespace TrafficLightCoordinatorStore.Publishers.Logs;

public interface ITrafficLogPublisher
{
    Task PublishAuditAsync(string message, CancellationToken ct);
    Task PublishErrorAsync(string message, Exception exception, CancellationToken ct);
}
