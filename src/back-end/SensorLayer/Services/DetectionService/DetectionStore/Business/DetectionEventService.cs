using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Detection;
using DetectionData.Repositories.EmergencyVehicle;
using DetectionData.Repositories.Incident;
using DetectionData.Repositories.PublicTransport;
using DetectionStore.Domain;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;
using DetectionStore.Publishers.Event;
using DetectionStore.Publishers.Logs;

namespace DetectionStore.Business
{
    public class DetectionEventService : IDetectionEventService
    {
        private readonly IEmergencyVehicleDetectionRepository _emergencyRepo;
        private readonly IPublicTransportDetectionRepository _transportRepo;
        private readonly IIncidentDetectionRepository _incidentRepo;
        private readonly IDetectionCacheRepository _cache;
        private readonly IDetectionEventPublisher _publisher;
        private readonly IDetectionLogPublisher _logPublisher;
        private readonly IMapper _mapper;
        private readonly IntersectionContext _intersection;

        public DetectionEventService(
            IEmergencyVehicleDetectionRepository emergencyRepo,
            IPublicTransportDetectionRepository transportRepo,
            IIncidentDetectionRepository incidentRepo,
            IDetectionCacheRepository cache,
            IDetectionEventPublisher publisher,
            IDetectionLogPublisher logPublisher,
            IMapper mapper,
            IntersectionContext intersection)
        {
            _emergencyRepo = emergencyRepo;
            _transportRepo = transportRepo;
            _incidentRepo = incidentRepo;
            _cache = cache;
            _publisher = publisher;
            _logPublisher = logPublisher;
            _mapper = mapper;
            _intersection = intersection;
        }

        public async Task<IEnumerable<DetectionEventResponse>> GetActiveEventsAsync()
        {
            var id = _intersection.Id;

            var emergency = await _cache.GetEmergencyDetectedAsync(id);
            var transport = await _cache.GetPublicTransportDetectedAsync(id);
            var incident = await _cache.GetIncidentDetectedAsync(id);

            var events = new List<DetectionEventResponse>();

            if (emergency == true)
                events.Add(new DetectionEventResponse { IntersectionId = id, EventType = "emergency", Detected = true, Timestamp = DateTime.UtcNow });

            if (transport == true)
                events.Add(new DetectionEventResponse { IntersectionId = id, EventType = "public_transport", Detected = true, Timestamp = DateTime.UtcNow });

            if (!string.IsNullOrEmpty(incident))
                events.Add(new DetectionEventResponse { IntersectionId = id, EventType = "incident", Detected = true, Description = incident, Timestamp = DateTime.UtcNow });

            await _logPublisher.PublishAuditAsync(
                "GET_ACTIVE_EVENTS",
                "Retrieved active detection events",
                new { IntersectionId = id, EventCount = events.Count });

            return events;
        }

        public async Task<DetectionEventResponse> ReportEventAsync(DetectionEventRequest request)
        {
            request.IntersectionId = _intersection.Id;
            var timestamp = DateTime.UtcNow;
            DetectionEventResponse response;

            switch (request.EventType.ToLowerInvariant())
            {
                case "emergency":
                    var ev = _mapper.Map<EmergencyVehicleDetection>(request);
                    ev.Timestamp = timestamp;
                    await _emergencyRepo.InsertAsync(ev);
                    await _cache.SetEmergencyDetectedAsync(request.IntersectionId, request.Detected);
                    await _publisher.PublishDetectionAsync("emergency");
                    response = _mapper.Map<DetectionEventResponse>(ev);
                    break;

                case "public_transport":
                    var pt = _mapper.Map<PublicTransportDetection>(request);
                    pt.Timestamp = timestamp;
                    await _transportRepo.InsertAsync(pt);
                    await _cache.SetPublicTransportDetectedAsync(request.IntersectionId, request.Detected);
                    await _publisher.PublishDetectionAsync("public_transport", request.Description);
                    response = _mapper.Map<DetectionEventResponse>(pt);
                    break;

                case "incident":
                    var inc = _mapper.Map<IncidentDetection>(request);
                    inc.Timestamp = timestamp;
                    await _incidentRepo.InsertAsync(inc);
                    await _cache.SetIncidentDetectedAsync(request.IntersectionId, request.Description ?? "");
                    await _publisher.PublishDetectionAsync("incident", request.Description);
                    response = _mapper.Map<DetectionEventResponse>(inc);
                    break;

                default:
                    throw new ArgumentException($"Unsupported event type: {request.EventType}");
            }

            await _logPublisher.PublishAuditAsync(
                "REPORT_EVENT",
                "Reported new detection event",
                new { request.IntersectionId, request.EventType, request.Description, request.Detected });

            return response;
        }
    }
}
