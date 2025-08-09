using System;

namespace EmergencyVehicleDetectionStore.Publishers;

public interface IEmergencyVehicleDetectionPublisher
{
    public Task PublishEmergencyVehicleDetectionAsync(Guid intersectionId, string vehicleId, string vehicleType, double speed, DateTime timestamp);
    public Task PublishAuditLogAsync(string message);
    public Task PublishErrorLogAsync(string message, Exception exception);
}
