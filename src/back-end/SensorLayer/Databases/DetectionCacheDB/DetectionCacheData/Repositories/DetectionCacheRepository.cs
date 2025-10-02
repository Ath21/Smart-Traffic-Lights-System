using System.Text.Json;
using DetectionCacheData.Keys;

namespace DetectionCacheData.Repositories;

public class DetectionCacheRepository : IDetectionCacheRepository
{
    private readonly DetectionCacheDbContext _context;

    public DetectionCacheRepository(DetectionCacheDbContext context)
    {
        _context = context;
    }

    // ==============================
    // VEHICLE
    // ==============================
    public async Task SetVehicleCountAsync(int intersectionId, string intersectionName, int count, double avgSpeed) =>
        await _context.SetValueAsync(DetectionCacheKeys.VehicleCount(intersectionId),
            JsonSerializer.Serialize(new { intersectionId, intersectionName, count, avgSpeed }));

    public async Task<string?> GetVehicleCountAsync(int intersectionId) =>
        await _context.GetValueAsync(DetectionCacheKeys.VehicleCount(intersectionId));

    // ==============================
    // PEDESTRIAN
    // ==============================
    public async Task SetPedestrianCountAsync(int intersectionId, string intersectionName, int count, string direction) =>
        await _context.SetValueAsync(DetectionCacheKeys.PedestrianCount(intersectionId),
            JsonSerializer.Serialize(new { intersectionId, intersectionName, count, direction }));

    public async Task<string?> GetPedestrianCountAsync(int intersectionId) =>
        await _context.GetValueAsync(DetectionCacheKeys.PedestrianCount(intersectionId));

    // ==============================
    // CYCLIST
    // ==============================
    public async Task SetCyclistCountAsync(int intersectionId, string intersectionName, int count, string direction) =>
        await _context.SetValueAsync(DetectionCacheKeys.CyclistCount(intersectionId),
            JsonSerializer.Serialize(new { intersectionId, intersectionName, count, direction }));

    public async Task<string?> GetCyclistCountAsync(int intersectionId) =>
        await _context.GetValueAsync(DetectionCacheKeys.CyclistCount(intersectionId));

    // ==============================
    // EMERGENCY
    // ==============================
    public async Task SetEmergencyDetectedAsync(int intersectionId, string intersectionName, bool detected, string type, int priorityLevel, string direction) =>
        await _context.SetValueAsync(DetectionCacheKeys.EmergencyDetected(intersectionId),
            JsonSerializer.Serialize(new { intersectionId, intersectionName, detected, type, priorityLevel, direction }));

    public async Task<string?> GetEmergencyDetectedAsync(int intersectionId) =>
        await _context.GetValueAsync(DetectionCacheKeys.EmergencyDetected(intersectionId));

    // ==============================
    // PUBLIC TRANSPORT
    // ==============================
    public async Task SetPublicTransportDetectedAsync(int intersectionId, string intersectionName, bool detected, string mode, string direction) =>
        await _context.SetValueAsync(DetectionCacheKeys.PublicTransportDetected(intersectionId),
            JsonSerializer.Serialize(new { intersectionId, intersectionName, detected, mode, direction }));

    public async Task<string?> GetPublicTransportDetectedAsync(int intersectionId) =>
        await _context.GetValueAsync(DetectionCacheKeys.PublicTransportDetected(intersectionId));

    // ==============================
    // INCIDENT
    // ==============================
    public async Task SetIncidentDetectedAsync(int intersectionId, string intersectionName, string type, int severity, string description, string direction) =>
        await _context.SetValueAsync(DetectionCacheKeys.IncidentDetected(intersectionId),
            JsonSerializer.Serialize(new { intersectionId, intersectionName, type, severity, description, direction }));

    public async Task<string?> GetIncidentDetectedAsync(int intersectionId) =>
        await _context.GetValueAsync(DetectionCacheKeys.IncidentDetected(intersectionId));
}
