namespace DetectionStore.Business;

public class SensorDetectionService : ISensorDetectionService
{
    private readonly IEmergencyVehicleDetectionRepository _emergencyRepo;
    private readonly PublicTransportDetectionRepository _transportRepo;
    private readonly IncidentDetectionRepository _incidentRepo;
    private readonly IDetectionEventPublisher _eventPublisher;
    private readonly IDetectionLogPublisher _logPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<SensorDetectionService> _logger;

    public SensorDetectionService(
        IEmergencyVehicleDetectionRepository emergencyRepo,
        PublicTransportDetectionRepository transportRepo,
        IncidentDetectionRepository incidentRepo,
        IDetectionEventPublisher eventPublisher,
        IDetectionLogPublisher logPublisher,
        IMapper mapper,
        ILogger<SensorDetectionService> logger)
    {
        _emergencyRepo = emergencyRepo;
        _transportRepo = transportRepo;
        _incidentRepo = incidentRepo;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DetectionSnapshotDto?> GetSnapshotAsync(Guid intersectionId)
    {
        var emergency = await _emergencyRepo.GetLatestAsync(intersectionId);
        var transport = await _transportRepo.GetLatestAsync(intersectionId);
        var incident = await _incidentRepo.GetLatestAsync(intersectionId);

        if (emergency == null && transport == null && incident == null)
            return null;

        return new DetectionSnapshotDto
        {
            IntersectionId = intersectionId,
            EmergencyVehicle = emergency != null ? _mapper.Map<EmergencyVehicleDto>(emergency) : null,
            PublicTransport = transport != null ? _mapper.Map<PublicTransportDto>(transport) : null,
            Incident = incident != null ? _mapper.Map<IncidentDto>(incident) : null,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<DetectionHistoryDto>> GetHistoryAsync(Guid intersectionId)
    {
        var emergency = await _emergencyRepo.GetHistoryAsync(intersectionId, 10);
        var transport = await _transportRepo.GetHistoryAsync(intersectionId, 10);
        var incident = await _incidentRepo.GetHistoryAsync(intersectionId, 10);

        var history = new List<DetectionHistoryDto>();

        // Align by timestamp or just flatten → for now we flatten
        history.AddRange(emergency.Select(e => new DetectionHistoryDto
        {
            Timestamp = e.Timestamp,
            EmergencyVehicle = _mapper.Map<EmergencyVehicleDto>(e)
        }));

        history.AddRange(transport.Select(t => new DetectionHistoryDto
        {
            Timestamp = t.Timestamp,
            PublicTransport = _mapper.Map<PublicTransportDto>(t)
        }));

        history.AddRange(incident.Select(i => new DetectionHistoryDto
        {
            Timestamp = i.Timestamp,
            Incident = _mapper.Map<IncidentDto>(i)
        }));

        return history.OrderByDescending(h => h.Timestamp);
    }

    public async Task<EmergencyVehicleDto> RecordEmergencyAsync(EmergencyVehicleDto dto)
    {
        var entity = _mapper.Map<EmergencyVehicleDetection>(dto);
        await _emergencyRepo.AddAsync(entity);

        await _eventPublisher.PublishEmergencyVehicleAsync(entity.IntersectionId, entity.Detected);
        await _logPublisher.PublishAuditAsync("RecordEmergency", $"Emergency={entity.Detected}", dto);

        return _mapper.Map<EmergencyVehicleDto>(entity);
    }

    public async Task<PublicTransportDto> RecordPublicTransportAsync(PublicTransportDto dto)
    {
        var entity = _mapper.Map<PublicTransportDetection>(dto);
        await _transportRepo.AddAsync(entity);

        await _eventPublisher.PublishPublicTransportAsync(entity.IntersectionId, entity.RouteId);
        await _logPublisher.PublishAuditAsync("RecordPublicTransport", $"Route={entity.RouteId}", dto);

        return _mapper.Map<PublicTransportDto>(entity);
    }

    public async Task<IncidentDto> RecordIncidentAsync(IncidentDto dto)
    {
        var entity = _mapper.Map<IncidentDetection>(dto);
        await _incidentRepo.AddAsync(entity);

        await _eventPublisher.PublishIncidentAsync(entity.IntersectionId, entity.Description);
        await _logPublisher.PublishAuditAsync("RecordIncident", $"Type={entity.Type}, Sev={entity.Severity}", dto);

        return _mapper.Map<IncidentDto>(entity);
    }
}