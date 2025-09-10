using System;
using AutoMapper;
using TrafficLightCacheData.Entities;
using TrafficLightControllerStore.Models.Dtos;
using TrafficLightControllerStore.Models.Requests;
using TrafficLightControllerStore.Models.Responses;

namespace TrafficLightControllerStore;

public class TrafficLightControlStoreProfile : Profile
{
    public TrafficLightControlStoreProfile()
    {
        // API ↔ Business
        CreateMap<UpdateLightRequest, TrafficLightDto>()
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.CurrentState));

        CreateMap<TrafficLightDto, TrafficLightStatusResponse>()
            .ForMember(dest => dest.CurrentState, opt => opt.MapFrom(src => src.State));

        CreateMap<ControlEventDto, ControlEventResponse>();

        // Business ↔ Redis Entities
        CreateMap<TrafficLightDto, TrafficLight>();
        CreateMap<TrafficLight, TrafficLightDto>();
    }
}
