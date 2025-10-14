using System;
using Messages.Traffic;

namespace TrafficLightCoordinatorStore.Engine;

public interface IDecisionEngine
{
    string EvaluatePriorityCount(PriorityCountMessage message);
    string EvaluatePriorityEvent(PriorityEventMessage message);
    string EvaluateTrafficAnalytics(TrafficAnalyticsMessage message);
}