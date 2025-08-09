using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using PublicTransportDetectionStore.Models;

namespace PublicTransportDetectionStore;

public class PublicTransportDetectionStoreProfile : Profile
{
    public PublicTransportDetectionStoreProfile()
    {
        // Entity → Read DTO
        CreateMap<PublicTransportDetection, PublicTransportDetectionReadDto>();
       
        // Create DTO → Entity
        CreateMap<PublicTransportDetectionCreateDto, PublicTransportDetection>();

        // Entity → Response DTO
        CreateMap<PublicTransportDetection, PublicTransportDetectionResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Success"));
    }
}
