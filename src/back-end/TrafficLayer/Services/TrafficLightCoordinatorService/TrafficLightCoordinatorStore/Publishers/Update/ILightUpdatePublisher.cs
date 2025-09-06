using TrafficMessages;

namespace TrafficLightCoordinatorStore.Publishers.Update;

public interface ILightUpdatePublisher
{
    Task PublishAsync(TrafficLightUpdateMessage message, CancellationToken ct);
}
