using System;

namespace TrafficLightCoordinatorStore.Business.Operator;

public interface ITrafficOperatorBusiness
{
    Task ApplyModeAsync(int intersectionId, string mode);
}

