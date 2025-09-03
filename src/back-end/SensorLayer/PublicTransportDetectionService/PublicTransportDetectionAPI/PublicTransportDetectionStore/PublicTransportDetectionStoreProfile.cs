using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using PublicTransportDetectionStore.Models;
using PublicTransportDetectionStore.Models.Requests;
using PublicTransportDetectionStore.Models.Responses;
using SensorMessages;

namespace PublicTransportDetectionStore;

public class PublicTransportDetectionStoreProfile : Profile
{
    public PublicTransportDetectionStoreProfile()
    {
        CreateMap<PublicTransportDetectionRequest, PublicTransportDetection>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow));

        CreateMap<PublicTransportDetection, PublicTransportDetectionResponse>();
        CreateMap<PublicTransportDetection, PublicTransportMessage>();
        CreateMap<PublicTransportMessage, PublicTransportDetection>();
    }
}
