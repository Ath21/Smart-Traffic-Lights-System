using AutoMapper;
using DetectionCacheData.Entities;
using SensorStore.Models.Dtos;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

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