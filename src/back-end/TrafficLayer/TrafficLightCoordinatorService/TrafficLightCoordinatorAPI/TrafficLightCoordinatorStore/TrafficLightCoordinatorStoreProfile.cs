using AutoMapper;
using TrafficLightCoordinatorData.Entities;
using TrafficLightCoordinatorStore.Models;
using System.Text.Json;

namespace TrafficLightCoordinatorStore;

public class TrafficLightCoordinatorStoreProfile : Profile
{
    public TrafficLightCoordinatorStoreProfile()
    {
        CreateMap<TrafficConfiguration, GetScheduleResponseDto>()
            .ForCtorParam("Intersection_Id", opt => opt.MapFrom(src => src.IntersectionId))
            .ForCtorParam("Schedule_Pattern", opt => opt.MapFrom(src =>
                ParsePattern(src.Pattern, src.UpdatedAt)));

        // helper conversions
        CreateMap<UpdateScheduleRequestDto, TrafficConfiguration>()
            .ForMember(d => d.Pattern, o => o.MapFrom(s => JsonSerializer.Serialize(
                new { phases = s.Schedule_Pattern.Phases, updated_at = DateTimeOffset.UtcNow })))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTimeOffset.UtcNow));
    }

    private static SchedulePatternDto ParsePattern(string json, DateTimeOffset updatedAt)
    {
        try
        {
            var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            var phases = new List<PhaseDto>();
            if (doc.RootElement.TryGetProperty("phases", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in arr.EnumerateArray())
                {
                    phases.Add(new PhaseDto(
                        p.GetProperty("phase").GetString() ?? "Unknown",
                        p.GetProperty("duration").GetInt32()));
                }
            }
            return new SchedulePatternDto(phases, updatedAt);
        }
        catch
        {
            return new SchedulePatternDto(new(), updatedAt);
        }
    }
}
