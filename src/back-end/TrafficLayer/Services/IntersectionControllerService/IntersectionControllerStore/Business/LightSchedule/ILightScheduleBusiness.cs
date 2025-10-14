using System;
using Messages.Traffic;

namespace IntersectionControllerStore.Business.LightSchedule;

public interface ILightScheduleBusiness
{
    Task ProcessScheduleAsync(TrafficLightScheduleMessage schedule);
}
