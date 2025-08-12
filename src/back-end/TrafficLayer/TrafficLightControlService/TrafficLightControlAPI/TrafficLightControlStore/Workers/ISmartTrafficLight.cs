using System;
using TrafficMessages.Light;

namespace TrafficLightControlStore.Workers;

public interface ISmartTrafficLight
{
    Task ApplyControlAsync(TrafficLightControl control);
}