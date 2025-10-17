using TrafficLightCacheData.Repositories;
using TrafficLightControllerStore.Publishers.Logs;

namespace TrafficLightControllerStore.Business.Failover;

public class FailoverBusiness : IFailoverBusiness
{
    private readonly ITrafficLightCacheRepository _cache;
    private readonly ITrafficLightLogPublisher _logPublisher;
    private readonly ILogger<FailoverBusiness> _logger;

    public FailoverBusiness(
        ITrafficLightCacheRepository cache,
        ITrafficLightLogPublisher logPublisher,
        ILogger<FailoverBusiness> logger)
    {
        _cache = cache;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    // ============================================================
    // ACTIVATE FAILOVER MODE
    // ============================================================
    public async Task ActivateFailoverAsync(int intersectionId, int lightId, string reason)
    {
        _logger.LogWarning(
            "[BUSINESS][FAILOVER][{Intersection}/{Light}] Activating local failover (Reason={Reason})",
            intersectionId, lightId, reason);

        await _cache.SetFailoverAsync(intersectionId, lightId, true);

        var metadata = new Dictionary<string, string>
        {
            ["intersection_id"] = intersectionId.ToString(),
            ["light_id"] = lightId.ToString(),
            ["reason"] = reason,
            ["activated_at"] = DateTime.UtcNow.ToString("o")
        };

        await _logPublisher.PublishFailoverAsync(
            action: "FAILOVER_ACTIVATED",
            message: $"Failover activated for Light {lightId} at Intersection {intersectionId}",
            metadata: metadata);
    }

    // ============================================================
    // DEACTIVATE FAILOVER MODE
    // ============================================================
    public async Task DeactivateFailoverAsync(int intersectionId, int lightId, string recoverySource)
    {
        _logger.LogInformation(
            "[BUSINESS][FAILOVER][{Intersection}/{Light}] Deactivating failover (RecoverySource={Source})",
            intersectionId, lightId, recoverySource);

        await _cache.SetFailoverAsync(intersectionId, lightId, false);

        var metadata = new Dictionary<string, string>
        {
            ["intersection_id"] = intersectionId.ToString(),
            ["light_id"] = lightId.ToString(),
            ["recovery_source"] = recoverySource,
            ["recovered_at"] = DateTime.UtcNow.ToString("o")
        };

        await _logPublisher.PublishFailoverAsync(
            action: "FAILOVER_DEACTIVATED",
            message: $"Failover deactivated for Light {lightId} at Intersection {intersectionId}",
            metadata: metadata);
    }

    // ============================================================
    // CHECK FAILOVER STATE
    // ============================================================
    public async Task<bool> IsFailoverActiveAsync(int intersectionId, int lightId)
    {
        var active = await _cache.GetFailoverAsync(intersectionId, lightId);
        _logger.LogDebug(
            "[BUSINESS][FAILOVER][{Intersection}/{Light}] Current failover state: {State}",
            intersectionId, lightId, active);
        return active;
    }
}
