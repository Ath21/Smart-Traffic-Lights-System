using System;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Publishers.Summary;

public interface ITrafficSummaryPublisher
{
    Task PublishSummaryAsync(TrafficSummaryMessage message);
}