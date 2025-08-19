using System;
using TrafficLightCoordinatorStore.Models;

namespace TrafficLightCoordinatorStore.Business.Coordination;

/// <summary>
/// Tiny placeholder algorithm: bumps phase durations depending on priority type.
/// Swap with your real optimizer.
/// </summary>
public static class CoordinationEngine
{
    public static List<PhaseDto> Recalculate(List<PhaseDto> basePhases, string priorityType)
    {
        if (basePhases.Count == 0) return basePhases;

        // Copy
        basePhases = basePhases.Select(p => new PhaseDto(p.Phase, p.Duration)).ToList();

        var bump = priorityType switch
        {
            "emergency" => 15,
            "public_transport" => 8,
            "pedestrian" => 6,
            "cyclist" => 6,
            _ => 0
        };

        // Naive: extend the first phase
        basePhases[0] = basePhases[0] with { Duration = Math.Max(5, basePhases[0].Duration + bump) };

        return basePhases;
    }
}
