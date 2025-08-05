using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using VehicleDetectionStore.Models;

namespace VehicleDetectionStore;

public class VehicleDetectionStoreProfile : Profile
{
    public VehicleDetectionStoreProfile()
    {
        CreateMap<VehicleDetectionCreateDto, VehicleDetection>()
            .ForMember(dest => dest.DetectionId, opt => opt.MapFrom(_ => Guid.NewGuid()));

        CreateMap<VehicleDetection, VehicleDetectionReadDto>();
    }
}
