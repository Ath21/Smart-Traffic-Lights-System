using static TrafficLightCoordinatorStore.Models.UpdateScheduleRequestDto;

namespace TrafficLightCoordinatorStore.Models;

public record UpdateScheduleRequestDto(SchedulePatternOnly Schedule_Pattern)
{
    public record SchedulePatternOnly(List<PhaseDto> Phases);
}