using System;
using AutoMapper;
using IntersectionControllerData.Entities;
using TrafficLightControlStore.Models.Dtos;
using TrafficLightControlStore.Models.Requests;
using TrafficLightControlStore.Models.Responses;

namespace TrafficLightControlStore;

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
