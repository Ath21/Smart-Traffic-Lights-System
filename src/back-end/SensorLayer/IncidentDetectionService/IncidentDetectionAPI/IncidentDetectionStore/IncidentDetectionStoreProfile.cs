using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using IncidentDetectionStore.Models;

namespace IncidentDetectionStore;

public class IncidentDetectionStoreProfile : Profile
{
    public IncidentDetectionStoreProfile()
    {
        CreateMap<IncidentDetectionCreateDto, IncidentDetection>();
        CreateMap<IncidentDetection, IncidentDetectionReadDto>();
        CreateMap<IncidentDetection, IncidentDetectionResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Created"));
    }
}