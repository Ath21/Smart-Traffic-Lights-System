using DetectionCacheData.Keys;

namespace DetectionCacheData.Repositories;

public class DetectionCacheRepository : IDetectionCacheRepository
{
    private readonly DetectionCacheDbContext _context;

    public DetectionCacheRepository(DetectionCacheDbContext context)
    {
        _context = context;
    }

    // ===============================
    // Vehicle Count
    // ===============================
    public async Task SetVehicleCountAsync(int intersectionId, int count)
        => await _context.SetValueAsync(DetectionCacheKeys.VehicleCount(intersectionId), count.ToString());

    public async Task<int> GetVehicleCountAsync(int intersectionId)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.VehicleCount(intersectionId));
        return int.TryParse(value, out var result) ? result : 0;
    }

    // ===============================
    // Pedestrian Count
    // ===============================
    public async Task SetPedestrianCountAsync(int intersectionId, int count)
        => await _context.SetValueAsync(DetectionCacheKeys.PedestrianCount(intersectionId), count.ToString());

    public async Task<int> GetPedestrianCountAsync(int intersectionId)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.PedestrianCount(intersectionId));
        return int.TryParse(value, out var result) ? result : 0;
    }

    // ===============================
    // Cyclist Count
    // ===============================
    public async Task SetCyclistCountAsync(int intersectionId, int count)
        => await _context.SetValueAsync(DetectionCacheKeys.CyclistCount(intersectionId), count.ToString());

    public async Task<int> GetCyclistCountAsync(int intersectionId)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.CyclistCount(intersectionId));
        return int.TryParse(value, out var result) ? result : 0;
    }

    // ===============================
    // Emergency Flags
    // ===============================
    public async Task SetEmergencyDetectedAsync(int intersectionId, bool detected)
        => await _context.SetValueAsync(DetectionCacheKeys.EmergencyDetected(intersectionId), detected.ToString());

    public async Task<bool> GetEmergencyDetectedAsync(int intersectionId)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.EmergencyDetected(intersectionId));
        return bool.TryParse(value, out var result) && result;
    }

    // ===============================
    // Incident Flags
    // ===============================
    public async Task SetIncidentDetectedAsync(int intersectionId, bool detected)
        => await _context.SetValueAsync(DetectionCacheKeys.IncidentDetected(intersectionId), detected.ToString());

    public async Task<bool> GetIncidentDetectedAsync(int intersectionId)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.IncidentDetected(intersectionId));
        return bool.TryParse(value, out var result) && result;
    }

    // ===============================
    // Public Transport Flags
    // ===============================
    public async Task SetPublicTransportDetectedAsync(int intersectionId, bool detected)
        => await _context.SetValueAsync(DetectionCacheKeys.PublicTransportDetected(intersectionId), detected.ToString());

    public async Task<bool> GetPublicTransportDetectedAsync(int intersectionId)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.PublicTransportDetected(intersectionId));
        return bool.TryParse(value, out var result) && result;
    }
}
