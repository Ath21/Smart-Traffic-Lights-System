using System;
using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Count;
using DetectionData.Repositories.Cyclist;
using DetectionData.Repositories.Pedestrian;
using DetectionData.Repositories.Vehicle;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;
using SensorStore.Publishers.Count;

namespace SensorStore.Business;

public class SensorCountService : ISensorCountService
{
    private readonly IVehicleCountRepository _vehicleRepo;
    private readonly IPedestrianCountRepository _pedestrianRepo;
    private readonly ICyclistCountRepository _cyclistRepo;
    private readonly IDetectionCacheRepository _cacheRepo;
    private readonly ISensorCountPublisher _publisher;
    private readonly IMapper _mapper;

    public SensorCountService(
        IVehicleCountRepository vehicleRepo,
        IPedestrianCountRepository pedestrianRepo,
        ICyclistCountRepository cyclistRepo,
        IDetectionCacheRepository cacheRepo,
        ISensorCountPublisher publisher,
        IMapper mapper)
    {
        _vehicleRepo = vehicleRepo;
        _pedestrianRepo = pedestrianRepo;
        _cyclistRepo = cyclistRepo;
        _cacheRepo = cacheRepo;
        _publisher = publisher;
        _mapper = mapper;
    }

    public async Task<SensorResponse> GetSensorDataAsync(int intersectionId)
    {
        var vehicleCount = await _cacheRepo.GetVehicleCountAsync(intersectionId) ?? 0;
        var pedestrianCount = await _cacheRepo.GetPedestrianCountAsync(intersectionId) ?? 0;
        var cyclistCount = await _cacheRepo.GetCyclistCountAsync(intersectionId) ?? 0;
        var emergency = await _cacheRepo.GetEmergencyDetectedAsync(intersectionId) ?? false;
        var publicTransport = await _cacheRepo.GetPublicTransportDetectedAsync(intersectionId) ?? false;
        var incident = await _cacheRepo.GetIncidentDetectedAsync(intersectionId);

        return new SensorResponse
        {
            IntersectionId = intersectionId,
            VehicleCount = vehicleCount,
            PedestrianCount = pedestrianCount,
            CyclistCount = cyclistCount,
            EmergencyDetected = emergency,
            PublicTransportDetected = publicTransport,
            LastIncident = incident,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task ReportSensorDataAsync(SensorReportRequest dto)
    {
        // persist to MongoDB
        await _vehicleRepo.InsertAsync(_mapper.Map<VehicleCount>(dto));
        await _pedestrianRepo.InsertAsync(_mapper.Map<PedestrianCount>(dto));
        await _cyclistRepo.InsertAsync(_mapper.Map<CyclistCount>(dto));

        // update Redis
        await _cacheRepo.SetVehicleCountAsync(dto.IntersectionId, dto.VehicleCount);
        await _cacheRepo.SetPedestrianCountAsync(dto.IntersectionId, dto.PedestrianCount);
        await _cacheRepo.SetCyclistCountAsync(dto.IntersectionId, dto.CyclistCount);

        // publish events via publisher abstraction
        await _publisher.PublishVehicleCountAsync(dto.IntersectionId, dto.VehicleCount, avgSpeed: 0); // extend dto if needed
        await _publisher.PublishPedestrianCountAsync(dto.IntersectionId, dto.PedestrianCount);
        await _publisher.PublishCyclistCountAsync(dto.IntersectionId, dto.CyclistCount);
    }
}