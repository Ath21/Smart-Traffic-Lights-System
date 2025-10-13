using UserStore.Publishers.Traffic;

namespace UserStore.Business.Traffic;

public class TrafficService : ITrafficService
{
    private readonly ITrafficPublisher _trafficPublisher;
    private readonly ILogger<TrafficService> _logger;

    private const string ServiceTag = "[" + nameof(TrafficService) + "]";

    public TrafficService(ITrafficPublisher trafficPublisher, ILogger<TrafficService> logger)
    {
        _trafficPublisher = trafficPublisher;
        _logger = logger;
    }

    public async Task ControlLightAsync(Guid intersectionId, Guid lightId, string newState)
    {
        await _trafficPublisher.PublishControlAsync(intersectionId, lightId, newState);

        _logger.LogInformation(
            "{Tag} Control command issued: Intersection {IntersectionId}, Light {LightId}, NewState {State}",
            ServiceTag, intersectionId, lightId, newState
        );
    }

    public async Task UpdateLightAsync(Guid intersectionId, Guid lightId, string currentState)
    {
        await _trafficPublisher.PublishUpdateAsync(intersectionId, lightId, currentState);

        _logger.LogInformation(
            "{Tag} Update published: Intersection {IntersectionId}, Light {LightId}, State {State}",
            ServiceTag, intersectionId, lightId, currentState
        );
    }
}