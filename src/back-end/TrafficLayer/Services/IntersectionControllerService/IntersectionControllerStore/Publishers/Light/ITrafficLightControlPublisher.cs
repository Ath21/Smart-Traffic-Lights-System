using System;

namespace IntersectionControllerStore.Publishers.Light;

public interface ITrafficLightControlPublisher
{
    Task PublishLightControlAsync(
        int lightId,
        string lightName,
        string operationalMode,
        string timePlan,
        Dictionary<string, int> phaseDurations,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null);
}
