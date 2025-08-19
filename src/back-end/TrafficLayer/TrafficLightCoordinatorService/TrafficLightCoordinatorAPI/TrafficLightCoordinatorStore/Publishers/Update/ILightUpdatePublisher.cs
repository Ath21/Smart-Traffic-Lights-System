using System;
using TrafficLightCoordinatorStore.Models;

namespace TrafficLightCoordinatorStore.Publishers.Update;

public interface ILightUpdatePublisher
{
    Task PublishAsync(string intersectionId, string currentPattern, CancellationToken ct);
}
