using AutoMapper;
using DetectionData.TimeSeriesObjects; // or wherever your CyclistDetection entity is
using CyclistDetectionStore.Models;
using CyclistDetectionStore.Models.Requests;
using CyclistDetectionStore.Models.Responses;
using SensorMessages;

namespace CyclistDetectionStore;

public class CyclistDetectionStoreProfile : Profile
{
    public CyclistDetectionStoreProfile()
    {
        CreateMap<CyclistDetectionRequest, CyclistDetection>()
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow));

        CreateMap<CyclistDetection, CyclistDetectionResponse>();
        CreateMap<CyclistDetection, CyclistDetectionMessage>();
        CreateMap<CyclistDetectionMessage, CyclistDetection>();
    }
}
