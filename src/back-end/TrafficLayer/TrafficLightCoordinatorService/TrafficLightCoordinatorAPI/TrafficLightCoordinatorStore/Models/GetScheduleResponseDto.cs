using System;

namespace TrafficLightCoordinatorStore.Models;

public class GetScheduleResponseDto
{
    public Guid Intersection_Id { get; set; }
    public SchedulePatternDto Schedule_Pattern { get; set; } = new();
}
