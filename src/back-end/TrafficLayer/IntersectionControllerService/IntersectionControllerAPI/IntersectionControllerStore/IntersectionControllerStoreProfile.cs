using System;
using AutoMapper;
using IntersectionControllerData.Entities;
using IntersectionControllerStore.Models.Dtos;
using IntersectionControllerStore.Models.Responses;

namespace IntersectionControllerStore;

public class IntersectionControllerStoreProfile : Profile
{
    public IntersectionControllerStoreProfile()
    {
        // Entities -> Models
        CreateMap<Intersection, IntersectionDto>();
        CreateMap<TrafficLight, TrafficLightDto>();
        CreateMap<TrafficConfiguration, TrafficConfigurationDto>();

        // Models -> Response DTOs
        CreateMap<IntersectionDto, IntersectionResponse>();
        CreateMap<TrafficLightDto, TrafficLightStatusResponse>();
        CreateMap<TrafficConfigurationDto, TrafficConfigurationResponse>();
    }
}