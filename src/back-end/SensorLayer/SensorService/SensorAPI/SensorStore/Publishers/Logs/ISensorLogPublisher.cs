using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SensorStore.Publishers;

public interface ISensorLogPublisher
{
    Task PublishAuditAsync(string action, string details, object? metadata = null);
    Task PublishErrorAsync(string errorType, string message, object? metadata = null);
}

