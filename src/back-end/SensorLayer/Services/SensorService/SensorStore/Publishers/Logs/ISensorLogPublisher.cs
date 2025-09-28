using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SensorStore.Publishers.Logs;

public interface ISensorLogPublisher
{
    Task PublishAuditAsync(int intersectionId, string action, string details, object? metadata = null);
    Task PublishErrorAsync(int intersectionId, string errorType, string message, object? metadata = null);
}

