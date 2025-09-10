namespace SensorStore;

public class SensorStoreProfile : Profile
{
    public SensorStoreProfile()
    {
        // Entities ↔ DTO
        CreateMap<DetectionCache, SensorSnapshotDto>().ReverseMap();

        // DTO ↔ Response
        CreateMap<SensorSnapshotDto, SensorSnapshotResponse>();
        CreateMap<SensorHistoryDto, SensorHistoryResponse>();

        // Request ↔ DTO
        CreateMap<UpdateSensorSnapshotRequest, SensorSnapshotDto>();
    }
}