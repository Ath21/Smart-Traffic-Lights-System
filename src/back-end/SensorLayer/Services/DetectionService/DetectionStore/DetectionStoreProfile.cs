using AutoMapper;
using DetectionData.Collections.Detection;
using DetectionStore.Models.Dtos;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore;

public class DetectionStoreProfile : Profile
{
    public DetectionStoreProfile()
    {
        // Request → Entities
        CreateMap<DetectionEventRequest, EmergencyVehicleDetection>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.SubType ?? "unknown"));

        CreateMap<DetectionEventRequest, PublicTransportDetection>()
            .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.SubType ?? "unknown"))
            .ForMember(dest => dest.RouteId, opt => opt.MapFrom(src => src.Description ?? string.Empty));

        CreateMap<DetectionEventRequest, IncidentDetection>();

        // Worker DTOs → Entities
        CreateMap<EmergencyVehicleDto, EmergencyVehicleDetection>();
        CreateMap<PublicTransportDto, PublicTransportDetection>();
        CreateMap<IncidentDto, IncidentDetection>();

        // Entities → Response
        CreateMap<EmergencyVehicleDetection, DetectionEventResponse>();
        CreateMap<PublicTransportDetection, DetectionEventResponse>();
        CreateMap<IncidentDetection, DetectionEventResponse>();
    }
}
