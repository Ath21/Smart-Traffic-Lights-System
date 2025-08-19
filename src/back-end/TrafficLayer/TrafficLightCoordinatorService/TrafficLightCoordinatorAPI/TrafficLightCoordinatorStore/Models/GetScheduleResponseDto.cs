namespace TrafficLightCoordinatorStore.Models;

public record GetScheduleResponseDto(
    Guid Intersection_Id,
    SchedulePatternDto Schedule_Pattern);
