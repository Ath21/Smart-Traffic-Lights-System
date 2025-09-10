namespace SensorStore.Business;

public interface ISensorCountService
{
    Task<SensorSnapshotDto?> GetSnapshotAsync(Guid intersectionId);
    Task<IEnumerable<SensorHistoryDto>> GetHistoryAsync(Guid intersectionId);
    Task<SensorSnapshotDto> UpdateSnapshotAsync(SensorSnapshotDto snapshot, float avgSpeed);
}