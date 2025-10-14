using System;
using Messages.Traffic;

namespace TrafficLightCoordinatorStore.Business;

public interface ICoordinatorBusiness
{
    Task HandlePriorityCountAsync(PriorityCountMessage message);
    Task HandlePriorityEventAsync(PriorityEventMessage message);
    Task HandleTrafficAnalyticsAsync(TrafficAnalyticsMessage message);
}
