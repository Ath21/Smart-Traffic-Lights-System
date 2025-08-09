using System;

namespace PublicTransportDetectionStore.Publishers;

public interface IPublicTransportDetectionPublisher
{
    /// <summary>
    /// Publishes a public transport detection event.
    /// </summary>
    Task PublishPublicTransportDetectionAsync(
        Guid intersectionId,
        string routeId,
        string vehicleType,
        int passengerCount,
        DateTime timestamp);

    /// <summary>
    /// Publishes an audit log message.
    /// </summary>
    Task PublishAuditLogAsync(string message);

    /// <summary>
    /// Publishes an error log message.
    /// </summary>
    Task PublishErrorLogAsync(string message, Exception exception);
}