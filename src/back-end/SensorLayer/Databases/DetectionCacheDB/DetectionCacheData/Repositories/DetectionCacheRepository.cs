using System;
using DetectionCacheData.Cache;

namespace DetectionCacheData.Repositories;

public class DetectionCacheRepository : IDetectionCacheRepository
{
    private readonly DetectionCacheDbContext _context;

    public DetectionCacheRepository(DetectionCacheDbContext context)
    {
        _context = context;
    }

    public async Task SetVehicleCountAsync(int intersection, int count) =>
        await _context.SetValueAsync(DetectionCacheKeys.VehicleCount(intersection), count.ToString());

    public async Task<int?> GetVehicleCountAsync(int intersection)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.VehicleCount(intersection));
        return value is null ? null : int.Parse(value);
    }

    public async Task SetPedestrianCountAsync(int intersection, int count) =>
        await _context.SetValueAsync(DetectionCacheKeys.PedestrianCount(intersection), count.ToString());

    public async Task<int?> GetPedestrianCountAsync(int intersection)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.PedestrianCount(intersection));
        return value is null ? null : int.Parse(value);
    }

    public async Task SetCyclistCountAsync(int intersection, int count) =>
        await _context.SetValueAsync(DetectionCacheKeys.CyclistCount(intersection), count.ToString());

    public async Task<int?> GetCyclistCountAsync(int intersection)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.CyclistCount(intersection));
        return value is null ? null : int.Parse(value);
    }

    public async Task SetEmergencyDetectedAsync(int intersection, bool detected) =>
        await _context.SetValueAsync(DetectionCacheKeys.EmergencyDetected(intersection), detected.ToString());

    public async Task<bool?> GetEmergencyDetectedAsync(int intersection)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.EmergencyDetected(intersection));
        return value is null ? null : bool.Parse(value);
    }

    public async Task SetPublicTransportDetectedAsync(int intersection, bool detected) =>
        await _context.SetValueAsync(DetectionCacheKeys.PublicTransportDetected(intersection), detected.ToString());

    public async Task<bool?> GetPublicTransportDetectedAsync(int intersection)
    {
        var value = await _context.GetValueAsync(DetectionCacheKeys.PublicTransportDetected(intersection));
        return value is null ? null : bool.Parse(value);
    }

    public async Task SetIncidentDetectedAsync(int intersection, string jsonIncident) =>
        await _context.SetValueAsync(DetectionCacheKeys.IncidentDetected(intersection), jsonIncident);

    public async Task<string?> GetIncidentDetectedAsync(int intersection) =>
        await _context.GetValueAsync(DetectionCacheKeys.IncidentDetected(intersection));
}