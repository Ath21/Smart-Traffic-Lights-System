using System;
using Messages.Traffic;

namespace TrafficLightControllerStore.Business.LightControl;

public interface ITrafficLightControlBusiness
{
    Task ApplyControlMessageAsync(TrafficLightControlMessage msg);
}
