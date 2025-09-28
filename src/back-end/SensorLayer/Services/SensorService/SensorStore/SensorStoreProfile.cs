using AutoMapper;
using DetectionData.Collections.Count;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore;

public class SensorStoreProfile : Profile
{
    public SensorStoreProfile()
    {
        CreateMap<SensorReportRequest, VehicleCount>()
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.VehicleCount));

        CreateMap<SensorReportRequest, PedestrianCount>()
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.PedestrianCount));

        CreateMap<SensorReportRequest, CyclistCount>()
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.CyclistCount));
    }
}