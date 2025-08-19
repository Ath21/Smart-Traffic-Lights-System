namespace TrafficLightCoordinatorStore.Models;

public record SchedulePatternDto(
    List<PhaseDto> Phases,
    DateTimeOffset UpdatedAt);