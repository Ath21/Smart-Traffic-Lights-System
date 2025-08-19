using AutoMapper;
using System.Text.Json;
using TrafficLightCoordinatorData.Entities;
using TrafficLightCoordinatorStore.Models;


public class TrafficLightCoordinatorStoreProfile : Profile
{
    public TrafficLightCoordinatorStoreProfile()
    {
        // Entity -> DTO (simple conversion using a delegate)
        CreateMap<TrafficConfiguration, GetScheduleResponseDto>()
            .ConvertUsing(src => new GetScheduleResponseDto(
                src.IntersectionId,
                ParsePattern(src.Pattern, src.UpdatedAt)
            ));

        // Request -> Entity (build a new entity; no expression trees)
        CreateMap<UpdateScheduleRequestDto, TrafficConfiguration>()
            .ConstructUsing(src => new TrafficConfiguration
            {
                ConfigId = Guid.NewGuid(),
                // IntersectionId should be set in the service where you know it from the route
                // (or pass it via ctx.Options.Items if you prefer)
                Pattern   = BuildPatternJson(src.Schedule_Pattern.Phases),
                UpdatedAt = DateTimeOffset.UtcNow
            });
    }

    // ---------- helpers (plain methods; safe to call from ConvertUsing/ConstructUsing) ----------
    private static SchedulePatternDto ParsePattern(string? json, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new SchedulePatternDto(new(), updatedAt);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var phases = new List<PhaseDto>();

            if (doc.RootElement.TryGetProperty("phases", out var arr) &&
                arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in arr.EnumerateArray())
                {
                    var phase = p.TryGetProperty("phase", out var ph) ? ph.GetString() ?? "Unknown" : "Unknown";
                    var dur   = p.TryGetProperty("duration", out var du) ? du.GetInt32() : 0;
                    phases.Add(new PhaseDto(phase, dur));
                }
            }

            return new SchedulePatternDto(phases, updatedAt);
        }
        catch
        {
            return new SchedulePatternDto(new(), updatedAt);
        }
    }

    private static string BuildPatternJson(List<PhaseDto> phases)
    {
        var payload = new
        {
            phases = phases.Select(p => new { phase = p.Phase, duration = p.Duration }),
            updated_at = DateTimeOffset.UtcNow
        };
        return JsonSerializer.Serialize(payload);
    }
}
