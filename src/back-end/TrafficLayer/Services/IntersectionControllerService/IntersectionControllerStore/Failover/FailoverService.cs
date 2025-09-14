using System;
using IntersectionControllerStore.Publishers.LogPub;

namespace IntersectionControllerStore.Failover;

public class FailoverService : IFailoverService
{
    private readonly ILogger<FailoverService> _logger;
    private readonly ITrafficLogPublisher _logPublisher;

    public FailoverService(
        ILogger<FailoverService> logger,
        ITrafficLogPublisher logPublisher)
    {
        _logger = logger;
        _logPublisher = logPublisher;
    }

    /// <summary>
    /// Handles intersection-level failure.
    /// Lights remain autonomous using LightCacheDB state.
    /// </summary>
    public async Task HandleIntersectionFailureAsync(string intersection, string reason)
    {
        _logger.LogWarning(
            "[FailoverService] Intersection {Intersection} failed ({Reason}). Lights will continue with cached states.",
            intersection, reason);

        await _logPublisher.PublishFailoverAsync(
            serviceName: "IntersectionControllerService",
            context: intersection,        // this identifies the scope (intersection id)
            reason: reason,
            mode: "AutonomousMode",
            intersectionId: intersection, // used for routing key substitution
            metadata: new { Intersection = intersection, AppliedAt = DateTime.UtcNow }
        );
    }

}
