using System;

namespace TrafficLightControllerStore.Business.Failover;

public interface IFailoverBusiness
{
    Task ActivateFailoverAsync(int intersectionId, int lightId, string reason);
    Task DeactivateFailoverAsync(int intersectionId, int lightId, string recoverySource);
}
