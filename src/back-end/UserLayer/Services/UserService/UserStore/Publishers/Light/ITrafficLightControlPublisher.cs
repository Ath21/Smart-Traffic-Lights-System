using System;

namespace UserStore.Publishers.Light;

public interface ITrafficLightControlPublisher
{
    Task PublishControlAsync(
        int lightId,
        string lightName,
        string mode,
        string timePlan,
        Dictionary<string, int> phaseDurations,
        string operationalMode,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null);
}
