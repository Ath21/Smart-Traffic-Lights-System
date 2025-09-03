using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using IncidentDetectionStore.Models;
using IncidentDetectionStore.Models.Requests;
using IncidentDetectionStore.Models.Responses;
using SensorMessages;

namespace IncidentDetectionStore;

public class IncidentDetectionStoreProfile : Profile
{
    public IncidentDetectionStoreProfile()
    {
        // Request → Domain
        CreateMap<IncidentDetectionRequest, IncidentDetection>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow));

        // Domain → Response
        CreateMap<IncidentDetection, IncidentDetectionResponse>();

        // Domain → Message
        CreateMap<IncidentDetection, IncidentDetectionMessage>();

        // Message → Domain
        CreateMap<IncidentDetectionMessage, IncidentDetection>()
            .ForMember(dest => dest.DetectionId, opt => opt.MapFrom(src => src.DetectionId))
            .ForMember(dest => dest.IntersectionId, opt => opt.MapFrom(src => src.IntersectionId))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
    }
}