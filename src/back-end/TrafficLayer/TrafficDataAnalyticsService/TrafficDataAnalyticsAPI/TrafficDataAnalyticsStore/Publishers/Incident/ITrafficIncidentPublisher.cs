using System;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Publishers.Incident;

public interface ITrafficIncidentPublisher
{
    Task PublishIncidentAsync(TrafficIncidentMessage message);
}