using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Detection;
using DetectionData.Repositories;
using DetectionData.Repositories.EmergencyVehicle;
using DetectionData.Repositories.Incident;
using DetectionData.Repositories.PublicTransport;
using DetectionStore.Models.Dtos;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;
using DetectionStore.Publishers.Event;

namespace DetectionStore.Business;

public class DetectionEventService : IDetectionEventService
{
    private readonly IEmergencyVehicleDetectionRepository _emergencyRepo;
    private readonly IPublicTransportDetectionRepository _transportRepo;
    private readonly IIncidentDetectionRepository _incidentRepo;
    private readonly IDetectionCacheRepository _cache;
    private readonly IDetectionEventPublisher _publisher;
    private readonly IMapper _mapper;

    public DetectionEventService(
        IEmergencyVehicleDetectionRepository emergencyRepo,
        IPublicTransportDetectionRepository transportRepo,
        IIncidentDetectionRepository incidentRepo,
        IDetectionCacheRepository cache,
        IDetectionEventPublisher publisher,
        IMapper mapper)
    {
        _emergencyRepo = emergencyRepo;
        _transportRepo = transportRepo;
        _incidentRepo = incidentRepo;
        _cache = cache;
        _publisher = publisher;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DetectionEventResponse>> GetActiveEventsAsync(int intersectionId)
    {
        var emergency = await _cache.GetEmergencyDetectedAsync(intersectionId);
        var transport = await _cache.GetPublicTransportDetectedAsync(intersectionId);
        var incident = await _cache.GetIncidentDetectedAsync(intersectionId);

        var events = new List<DetectionEventResponse>();

        if (emergency == true)
            events.Add(new DetectionEventResponse
            {
                IntersectionId = intersectionId,
                EventType = "emergency",
                Detected = true,
                Timestamp = DateTime.UtcNow
            });

        if (transport == true)
            events.Add(new DetectionEventResponse
            {
                IntersectionId = intersectionId,
                EventType = "public_transport",
                Detected = true,
                Timestamp = DateTime.UtcNow
            });

        if (!string.IsNullOrEmpty(incident))
            events.Add(new DetectionEventResponse
            {
                IntersectionId = intersectionId,
                EventType = "incident",
                Detected = true,
                Description = incident,
                Timestamp = DateTime.UtcNow
            });

        return events;
    }

    public async Task<DetectionEventResponse> ReportEventAsync(DetectionEventRequest request)
    {
        var timestamp = DateTime.UtcNow;
        DetectionEventResponse response;

        switch (request.EventType.ToLowerInvariant())
        {
            case "emergency":
                var ev = _mapper.Map<EmergencyVehicleDetection>(request);
                ev.Timestamp = timestamp;
                await _emergencyRepo.InsertAsync(ev);
                await _cache.SetEmergencyDetectedAsync(request.IntersectionId, request.Detected);
                await _publisher.PublishDetectionAsync(request.IntersectionId, "emergency");
                response = _mapper.Map<DetectionEventResponse>(ev);
                break;

            case "public_transport":
                var pt = _mapper.Map<PublicTransportDetection>(request);
                pt.Timestamp = timestamp;
                await _transportRepo.InsertAsync(pt);
                await _cache.SetPublicTransportDetectedAsync(request.IntersectionId, request.Detected);
                await _publisher.PublishDetectionAsync(request.IntersectionId, "public_transport", request.Description);
                response = _mapper.Map<DetectionEventResponse>(pt);
                break;

            case "incident":
                var inc = _mapper.Map<IncidentDetection>(request);
                inc.Timestamp = timestamp;
                await _incidentRepo.InsertAsync(inc);
                await _cache.SetIncidentDetectedAsync(request.IntersectionId, request.Description ?? "");
                await _publisher.PublishDetectionAsync(request.IntersectionId, "incident", request.Description);
                response = _mapper.Map<DetectionEventResponse>(inc);
                break;

            default:
                throw new ArgumentException($"Unsupported event type: {request.EventType}");
        }

        return response;
    }

    public async Task RecordEmergencyAsync(EmergencyVehicleDto dto)
    {
        var ev = _mapper.Map<EmergencyVehicleDetection>(dto);
        await _emergencyRepo.InsertAsync(ev);
        await _cache.SetEmergencyDetectedAsync(dto.IntersectionId, dto.Detected);
        await _publisher.PublishDetectionAsync(dto.IntersectionId, "emergency", dto.Type);
    }

    public async Task RecordPublicTransportAsync(PublicTransportDto dto)
    {
        var pt = _mapper.Map<PublicTransportDetection>(dto);
        await _transportRepo.InsertAsync(pt);
        await _cache.SetPublicTransportDetectedAsync(dto.IntersectionId, dto.Detected);
        await _publisher.PublishDetectionAsync(dto.IntersectionId, "public_transport", dto.RouteId);
    }

    public async Task RecordIncidentAsync(IncidentDto dto)
    {
        var inc = _mapper.Map<IncidentDetection>(dto);
        await _incidentRepo.InsertAsync(inc);
        await _cache.SetIncidentDetectedAsync(dto.IntersectionId, dto.Description ?? "");
        await _publisher.PublishDetectionAsync(dto.IntersectionId, "incident", dto.Description);
    }

}
