using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers.Event;

public interface IDetectionEventPublisher
{
    Task PublishDetectionAsync(int intersectionId, string eventType, string? details = null);
}
