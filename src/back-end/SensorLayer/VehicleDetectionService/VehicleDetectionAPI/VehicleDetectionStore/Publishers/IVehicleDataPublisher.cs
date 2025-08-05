using System;

namespace VehicleDetectionStore.Publishers;

public interface IVehicleDataPublisher
{
    Task PublishVehicleCountAsync(Guid intersectionId, int vehicleCount, double avgSpeed, DateTime timestamp);
    Task PublishAuditLogAsync(string message);
    Task PublishErrorLogAsync(string message, Exception ex);
}