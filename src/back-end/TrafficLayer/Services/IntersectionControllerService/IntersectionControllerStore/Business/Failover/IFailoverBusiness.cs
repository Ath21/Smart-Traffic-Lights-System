using System;

namespace IntersectionControllerStore.Business.Failover;

public interface IFailoverBusiness
{
    Task TriggerFailoverAsync(int intersectionId, string intersectionName, List<int> lightIds, string reason);
}
