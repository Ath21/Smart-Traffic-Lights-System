namespace TrafficLightCoordinatorStore.Publishers.Update;

public interface ILightUpdatePublisher
{
    Task PublishUpdateAsync(
        string intersectionName,
        bool isOperational,
        string currentMode,
        string timePlan,
        Dictionary<string, int> phaseDurations,
        int cycleDurationSec,
        int globalOffsetSec,
        Dictionary<int, int> lightOffsets,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null);
}
