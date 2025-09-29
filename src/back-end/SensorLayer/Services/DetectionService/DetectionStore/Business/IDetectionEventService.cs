using System;
using DetectionStore.Models.Dtos;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore.Business;

public interface IDetectionEventService
{
    Task<IEnumerable<DetectionEventResponse>> GetActiveEventsAsync();
    Task<DetectionEventResponse> ReportEventAsync(DetectionEventRequest request);
}
