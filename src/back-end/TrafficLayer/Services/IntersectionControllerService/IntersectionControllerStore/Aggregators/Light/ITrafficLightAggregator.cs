using System;
using Messages.Traffic.Light;

namespace IntersectionControllerStore.Aggregators.Light;

public interface ITrafficLightAggregator
{
    Task<TrafficLightControlMessage> BuildLightControlAsync(TrafficLightScheduleMessage schedule);
}