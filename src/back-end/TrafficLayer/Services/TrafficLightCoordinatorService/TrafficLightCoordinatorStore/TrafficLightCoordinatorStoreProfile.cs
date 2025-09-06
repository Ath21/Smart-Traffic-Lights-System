using AutoMapper;
using TrafficLightCoordinatorData.Entities;
using TrafficLightCoordinatorStore.Models.Dtos;
using TrafficLightCoordinatorStore.Models.Responses;

namespace TrafficLightCoordinatorStore;

public class TrafficLightCoordinatorStoreProfile : Profile
{
    public TrafficLightCoordinatorStoreProfile()
    {
        // Entity -> DTO
        CreateMap<TrafficConfiguration, ConfigDto>()
            .ForMember(dest => dest.Pattern, opt => opt.MapFrom(src => src.Pattern.ToString()));

        // DTO -> Response
        CreateMap<ConfigDto, ConfigResponse>();

        CreateMap<PriorityDto, PriorityOverrideResponse>()
            .ForMember(dest => dest.AppliedType, opt => opt.MapFrom(src => src.PriorityType))
            .ForMember(dest => dest.AppliedPattern, opt => opt.MapFrom(src => src.AppliedPattern))
            .ForMember(dest => dest.AppliedAt, opt => opt.MapFrom(src => src.AppliedAt));
    }
}
