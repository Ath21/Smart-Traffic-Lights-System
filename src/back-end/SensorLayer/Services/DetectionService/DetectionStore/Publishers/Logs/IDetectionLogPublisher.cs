using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DetectionStore.Publishers.Logs;

public interface IDetectionLogPublisher
{
    Task PublishAuditAsync(string action, string details, object? metadata = null);
    Task PublishErrorAsync(string errorType, string message, object? metadata = null);
}
