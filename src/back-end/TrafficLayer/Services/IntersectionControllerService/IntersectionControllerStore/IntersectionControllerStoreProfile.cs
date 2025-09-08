using System;
using AutoMapper;
using IntersectionControllerStore.Models.Dtos;
using IntersectionControllerStore.Models.Responses;
using TrafficLightCacheData.Entities;

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