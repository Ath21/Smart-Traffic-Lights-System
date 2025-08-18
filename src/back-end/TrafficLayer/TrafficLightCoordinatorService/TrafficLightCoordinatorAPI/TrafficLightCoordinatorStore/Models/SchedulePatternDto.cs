using System;
using System.Text.Json.Serialization;

namespace TrafficLightCoordinatorStore.Models;

public class SchedulePatternDto
{
    [JsonPropertyName("phases")]
    public IReadOnlyList<PhaseDto> Phases { get; set; } = Array.Empty<PhaseDto>();

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAtUtc { get; set; }
}
