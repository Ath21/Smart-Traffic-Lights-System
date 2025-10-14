using System;
using Messages.Sensor;

namespace IntersectionControllerStore.Business.Priority;

public interface IPriorityBusiness
{
    Task HandleDetectionEventAsync(DetectionEventMessage msg);
    Task HandleSensorCountAsync(SensorCountMessage msg);
}
