using System;

namespace IntersectionControllerStore.Failover;


public interface IFailoverService
{
    Task HandleIntersectionFailureAsync(string intersection, string reason);
}