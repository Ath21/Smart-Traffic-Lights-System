using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers.Event;

public interface IDetectionEventPublisher
{
    Task PublishDetectionAsync(string eventType, string? details = null);
}
