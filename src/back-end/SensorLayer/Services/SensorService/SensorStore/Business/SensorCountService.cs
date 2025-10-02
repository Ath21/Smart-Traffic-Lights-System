using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Count;
using DetectionData.Repositories.Cyclist;
using DetectionData.Repositories.Pedestrian;
using DetectionData.Repositories.Vehicle;
using SensorStore.Domain;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;
using SensorStore.Publishers.Count;
using SensorStore.Publishers.Logs;

namespace SensorStore.Business
{
    public class SensorCountService : ISensorCountService
    {
        private readonly IVehicleCountRepository _vehicleRepo;
        private readonly IPedestrianCountRepository _pedestrianRepo;
        private readonly ICyclistCountRepository _cyclistRepo;
        private readonly IDetectionCacheRepository _cacheRepo;
        private readonly ISensorCountPublisher _publisher;
        private readonly ISensorLogPublisher _logPublisher;
        private readonly IMapper _mapper;
        private readonly IntersectionContext _intersection;

        public SensorCountService(
            IVehicleCountRepository vehicleRepo,
            IPedestrianCountRepository pedestrianRepo,
            ICyclistCountRepository cyclistRepo,
            IDetectionCacheRepository cacheRepo,
            ISensorCountPublisher publisher,
            ISensorLogPublisher logPublisher,
            IMapper mapper,
            IntersectionContext intersection)
        {
            _vehicleRepo = vehicleRepo;
            _pedestrianRepo = pedestrianRepo;
            _cyclistRepo = cyclistRepo;
            _cacheRepo = cacheRepo;
            _publisher = publisher;
            _logPublisher = logPublisher;
            _mapper = mapper;
            _intersection = intersection;
        }

        public async Task<SensorResponse> GetSensorDataAsync()
        {
            var id = _intersection.Id;
            var name = _intersection.Name;

            var vehicleCountStr = await _cacheRepo.GetVehicleCountAsync(id);
            var vehicleCount = int.TryParse(vehicleCountStr, out var vCount) ? vCount : 0;
            var pedestrianCountStr = await _cacheRepo.GetPedestrianCountAsync(id);
            var pedestrianCount = int.TryParse(pedestrianCountStr, out var pCount) ? pCount : 0;
            var cyclistCountStr = await _cacheRepo.GetCyclistCountAsync(id);
            var cyclistCount = int.TryParse(cyclistCountStr, out var cCount) ? cCount : 0;

            var snapshot = new SensorResponse
            {
                IntersectionId = id,
                IntersectionName = name,
                VehicleCount = vehicleCount,
                PedestrianCount = pedestrianCount,
                CyclistCount = cyclistCount,
                Timestamp = DateTime.UtcNow
            };

            // 🔹 Log audit: snapshot retrieved
            await _logPublisher.PublishAuditAsync("GET_SENSOR_DATA", "Snapshot retrieved", new
            {
                snapshot.IntersectionId,
                snapshot.IntersectionName,
                snapshot.VehicleCount,
                snapshot.PedestrianCount,
                snapshot.CyclistCount
            });

            return snapshot;
        }

        public async Task ReportSensorDataAsync(SensorReportRequest dto)
        {
            dto.IntersectionId = _intersection.Id;

            // persist to MongoDB
            await _vehicleRepo.InsertAsync(_mapper.Map<VehicleCount>(dto));
            await _pedestrianRepo.InsertAsync(_mapper.Map<PedestrianCount>(dto));
            await _cyclistRepo.InsertAsync(_mapper.Map<CyclistCount>(dto));

            // update Redis
            await _cacheRepo.SetVehicleCountAsync(dto.IntersectionId, dto.IntersectionName, dto.VehicleCount, 0);
            await _cacheRepo.SetPedestrianCountAsync(dto.IntersectionId, dto.IntersectionName, dto.PedestrianCount, "sensor");
            await _cacheRepo.SetCyclistCountAsync(dto.IntersectionId, dto.IntersectionName, dto.CyclistCount, "sensor");

            // publish events via publisher abstraction
            await _publisher.PublishVehicleCountAsync(dto.VehicleCount, avgSpeed: 0, direction: dto.Direction);
            await _publisher.PublishPedestrianCountAsync(dto.PedestrianCount, dto.Direction);
            await _publisher.PublishCyclistCountAsync(dto.CyclistCount, dto.Direction);

            // 🔹 Log audit: sensor data reported
            await _logPublisher.PublishAuditAsync("REPORT_SENSOR_DATA", "New sensor data recorded", new
            {
                dto.IntersectionId,
                dto.IntersectionName,
                dto.VehicleCount,
                dto.PedestrianCount,
                dto.CyclistCount
            });
        }
    }
}
