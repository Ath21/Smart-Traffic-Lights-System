using DetectionData.TimeSeriesObjects;

namespace IncidentDetectionStore.Repositories
{
    public interface IIncidentDetectionRepository
    {
        Task<Guid> InsertAsync(IncidentDetection detection);
        Task<List<IncidentDetection>> QueryAsync(
            Guid? intersectionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null);
    }
}
