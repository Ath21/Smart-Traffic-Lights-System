using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DetectionData.TimeSeriesObjects;

namespace PedestrianDetectionStore.Repositories
{
    public interface IPedestrianDetectionRepository
    {
        /// <summary>
        /// Inserts a new pedestrian detection record into the database.
        /// </summary>
        /// <param name="detection">The pedestrian detection entity to insert.</param>
        /// <returns>The generated detection ID.</returns>
        Task<Guid> InsertAsync(PedestrianDetection detection);

        /// <summary>
        /// Queries pedestrian detections with optional filters.
        /// </summary>
        /// <param name="intersectionId">Optional intersection ID to filter results.</param>
        /// <param name="startTime">Optional start time for filtering results.</param>
        /// <param name="endTime">Optional end time for filtering results.</param>
        /// <param name="limit">Optional limit for the number of results.</param>
        /// <returns>A list of pedestrian detections.</returns>
        Task<List<PedestrianDetection>> QueryAsync(
            Guid? intersectionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null);
    }
}
