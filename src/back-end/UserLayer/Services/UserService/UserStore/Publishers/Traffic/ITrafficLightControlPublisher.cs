using System;

namespace UserStore.Publishers.Traffic;

public interface ITrafficLightControlPublisher
{
    Task PublishControlAsync(
        int intersectionId,
        string intersectionName,
        int lightId,
        string lightName,
        string? mode = null,
        string? timePlan = null,
        string? operationalMode = null,
        Dictionary<string, int>? phaseDurations = null,
        string? currentPhase = null,
        int remainingTimeSec = 0,
        int cycleDurationSec = 0,
        int localOffsetSec = 0,
        double cycleProgressSec = 0,
        int priorityLevel = 1,
        bool isFailoverActive = false,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
}
