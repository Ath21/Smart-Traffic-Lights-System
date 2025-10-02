using System;

namespace DetectionCacheData.Repositories;

public interface IDetectionCacheRepository
{
    // ==============================
    // VEHICLE
    // ==============================
    Task SetVehicleCountAsync(int intersectionId, string intersectionName, int count, double avgSpeed);
    Task<string?> GetVehicleCountAsync(int intersectionId);

    // ==============================
    // PEDESTRIAN
    // ==============================
    Task SetPedestrianCountAsync(int intersectionId, string intersectionName, int count, string direction);
    Task<string?> GetPedestrianCountAsync(int intersectionId);

    // ==============================
    // CYCLIST
    // ==============================
    Task SetCyclistCountAsync(int intersectionId, string intersectionName, int count, string direction);
    Task<string?> GetCyclistCountAsync(int intersectionId);

    // ==============================
    // EMERGENCY
    // ==============================
    Task SetEmergencyDetectedAsync(int intersectionId, string intersectionName, bool detected, string type, int priorityLevel, string direction);
    Task<string?> GetEmergencyDetectedAsync(int intersectionId);

    // ==============================
    // PUBLIC TRANSPORT
    // ==============================
    Task SetPublicTransportDetectedAsync(int intersectionId, string intersectionName, bool detected, string mode, string direction);
    Task<string?> GetPublicTransportDetectedAsync(int intersectionId);

    // ==============================
    // INCIDENT
    // ==============================
    Task SetIncidentDetectedAsync(int intersectionId, string intersectionName, string type, int severity, string description, string direction);
    Task<string?> GetIncidentDetectedAsync(int intersectionId);
}
