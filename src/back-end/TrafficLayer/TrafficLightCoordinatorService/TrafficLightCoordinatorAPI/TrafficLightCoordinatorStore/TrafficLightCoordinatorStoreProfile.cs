using System;
using System.Text.Json;
using AutoMapper;
using NetTopologySuite.Geometries;
using TrafficLightCoordinatorData.Entities;
using TrafficLightCoordinatorStore.Models;

namespace TrafficLightCoordinatorStore;

public class TrafficLightCoordinatorStoreProfile : Profile
{
    public TrafficLightCoordinatorStoreProfile()
    {
        // Point(X=lng, Y=lat) → LatLngDto(lat,lng)
        CreateMap<Point?, LatLngDto?>()
            .ConvertUsing(p => p is null ? null : new LatLngDto(p.Y, p.X));

        CreateMap<Intersection, IntersectionReadDto>()
            .ForMember(d => d.IntersectionId, cfg => cfg.MapFrom(s => s.Id))
            .ForMember(d => d.Location, cfg => cfg.MapFrom(s => s.Location));

        CreateMap<TrafficLight, TrafficLightReadDto>();

        CreateMap<TrafficConfiguration, TrafficConfigurationReadDto>()
            .ForMember(d => d.Pattern, cfg => cfg.MapFrom(s =>
                (s.Pattern ?? JsonDocument.Parse("{}")).RootElement));
    }
}