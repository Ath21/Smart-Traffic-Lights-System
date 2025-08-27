using System;
using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Incident;

public interface ITrafficIncidentPublisher
{
    Task PublishIncidentAsync(TrafficIncidentMessage message);
}