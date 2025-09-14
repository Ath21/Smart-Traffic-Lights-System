using System;

namespace TrafficLightControllerStore.Failover;

public interface IFailoverService
{
    Task ApplyFailoverAsync(string intersection, string light);
    Task ApplyFailoverIntersectionAsync(string intersection);
}