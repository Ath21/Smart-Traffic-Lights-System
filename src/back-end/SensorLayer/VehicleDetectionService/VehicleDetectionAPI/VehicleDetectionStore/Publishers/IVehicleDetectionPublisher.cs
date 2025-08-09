namespace VehicleDetectionStore.Publishers;

public interface IVehicleDetectionPublisher
{
    Task PublishVehicleCountAsync(Guid intersectionId, int vehicleCount, double avgSpeed, DateTime timestamp);
    Task PublishAuditLogAsync(string message);
    Task PublishErrorLogAsync(string message, Exception ex);
}