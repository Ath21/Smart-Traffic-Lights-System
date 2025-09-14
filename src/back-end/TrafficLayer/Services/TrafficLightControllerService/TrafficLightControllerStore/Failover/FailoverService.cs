using System;

namespace TrafficLightControllerStore.Failover;

public class FailoverService : IFailoverService
{
    private readonly ITrafficLightRepository _repository;
    private readonly ILogger<FailoverService> _logger;

    private const string FailoverState = "BlinkingYellow";

    public FailoverService(ITrafficLightRepository repository, ILogger<FailoverService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Apply failover for a single traffic light (safe mode).
    /// </summary>
    public async Task ApplyFailoverAsync(string intersection, string light)
    {
        _logger.LogWarning(
            "[FailoverService] Applying failover for {Intersection}-{Light} -> {State}",
            intersection, light, FailoverState);

        await _repository.SetLightStateAsync(intersection, light, FailoverState);
    }

    /// <summary>
    /// Apply failover for an entire intersection (all lights → safe mode).
    /// </summary>
    public async Task ApplyFailoverIntersectionAsync(string intersection)
    {
        _logger.LogWarning(
            "[FailoverService] Applying failover for all lights in intersection {Intersection} -> {State}",
            intersection, FailoverState);

        var lights = await _repository.GetAllStatesAsync(intersection);

        foreach (var light in lights.Keys)
        {
            await _repository.SetLightStateAsync(intersection, light, FailoverState);
        }
    }
}