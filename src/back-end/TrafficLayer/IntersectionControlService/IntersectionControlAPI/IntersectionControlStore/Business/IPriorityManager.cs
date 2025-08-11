using System;
using IntersectionControlStore.Models;

namespace IntersectionControlStore.Business;

public interface IPriorityManager
{
    IntersectionPriorityStatus? GetPriorityStatus(Guid intersectionId);
    Task OverridePriorityAsync(Guid intersectionId, IntersectionPriorityStatus overrideStatus, int durationSeconds);
    Task ProcessSensorMessageAsync(object sensorMessage);
}
