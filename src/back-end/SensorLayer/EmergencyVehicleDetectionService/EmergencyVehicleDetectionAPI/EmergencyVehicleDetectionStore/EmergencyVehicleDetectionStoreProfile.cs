using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using EmergencyVehicleDetectionStore.Models;

namespace EmergencyVehicleDetectionStore;

public class EmergencyVehicleDetectionStoreProfile : Profile
{
    public EmergencyVehicleDetectionStoreProfile()
    {
        CreateMap<EmergencyVehicleDetectionCreateDto, EmergencyVehicleDetection>();
        CreateMap<EmergencyVehicleDetection, EmergencyVehicleDetectionReadDto>();
        CreateMap<EmergencyVehicleDetection, DetectionResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Success"));
    }
}
