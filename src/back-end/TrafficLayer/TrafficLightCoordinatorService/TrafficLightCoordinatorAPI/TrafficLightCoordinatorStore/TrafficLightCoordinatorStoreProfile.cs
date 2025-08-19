using AutoMapper;
using NetTopologySuite.Geometries;
using System.Text.Json;
using TrafficLightCoordinatorData.Entities;
using TrafficLightCoordinatorStore.Models;

namespace TrafficLightCoordinatorStore;

public class TrafficLightCoordinatorStoreProfile : Profile
{
    public TrafficLightCoordinatorStoreProfile()
    {
        //
        // LatLng <-> Point
        //
        CreateMap<Point, LatLngDto>()
            .ConvertUsing(p => p == null ? null : new LatLngDto { Lat = p.Y, Lng = p.X });

        CreateMap<LatLngDto, Point>()
            .ConvertUsing(d => d == null ? null : new Point(d.Lng, d.Lat) { SRID = 4326 });

        //
        // Entities -> DTOs
        //
        CreateMap<IntersectionEntity, IntersectionReadDto>();
        CreateMap<TrafficLightEntity, TrafficLightReadDto>();
        CreateMap<TrafficConfigurationEntity, TrafficConfigurationReadDto>()
            .ConvertUsing(s => new TrafficConfigurationReadDto
            {
                Id = s.Id,
                IntersectionId = s.IntersectionId,
                Pattern = s.Pattern == null
                    ? JsonDocument.Parse("{}").RootElement
                    : s.Pattern.RootElement,
                EffectiveFrom = s.EffectiveFrom
            });

        //
        // DTOs -> Entities
        //
        CreateMap<IntersectionCreateDto, IntersectionEntity>()
            .ForMember(e => e.Id, opt => opt.Ignore())
            .ForMember(e => e.Location, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                if (src.Lat.HasValue && src.Lng.HasValue)
                {
                    dest.Location = new Point(src.Lng.Value, src.Lat.Value) { SRID = 4326 };
                }
            });

        CreateMap<TrafficConfigurationCreateDto, TrafficConfiguration>()
            .ForMember(e => e.Id, opt => opt.Ignore())
            .ForMember(e => e.Pattern, opt => opt.MapFrom(src =>
                JsonDocument.Parse(src.Pattern.GetRawText())));
    }
}
