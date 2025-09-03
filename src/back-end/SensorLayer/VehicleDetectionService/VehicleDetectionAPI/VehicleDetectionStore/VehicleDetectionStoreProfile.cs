using AutoMapper;
using DetectionData.TimeSeriesObjects;
using VehicleDetectionStore.Models.Requests;
using VehicleDetectionStore.Models.Responses;
using SensorMessages;

namespace VehicleDetectionStore;

public class VehicleDetectionStoreProfile : Profile
{
    public VehicleDetectionStoreProfile()
    {
        // Request → Domain
        CreateMap<VehicleDetectionRequest, VehicleDetection>()
            .ForMember(dest => dest.Timestamp,
                       opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow));

        // Domain → Response
        CreateMap<VehicleDetection, VehicleDetectionResponse>();

        // Domain → Message
        CreateMap<VehicleDetection, VehicleCountMessage>();

        // Message → Domain
        CreateMap<VehicleCountMessage, VehicleDetection>()
            .ForMember(dest => dest.DetectionId, opt => opt.MapFrom(src => src.DetectionId))
            .ForMember(dest => dest.IntersectionId, opt => opt.MapFrom(src => src.IntersectionId))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp));
    }
}
