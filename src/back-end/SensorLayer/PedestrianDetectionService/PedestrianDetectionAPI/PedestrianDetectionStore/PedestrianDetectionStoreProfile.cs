using AutoMapper;
using DetectionData.TimeSeriesObjects;
using PedestrianDetectionStore.Models;
using PedestrianDetectionStore.Models.Requests;
using PedestrianDetectionStore.Models.Responses;
using SensorMessages;

namespace PedestrianDetectionStore;

public class PedestrianDetectionStoreProfile : Profile
{
    public PedestrianDetectionStoreProfile()
    {
        CreateMap<PedestrianDetectionRequest, PedestrianDetection>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow));

        CreateMap<PedestrianDetection, PedestrianDetectionResponse>();
        CreateMap<PedestrianDetection, PedestrianDetectionMessage>();
        CreateMap<PedestrianDetectionMessage, PedestrianDetection>();
    }
}
