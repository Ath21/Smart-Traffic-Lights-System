using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Incident;

public interface ITrafficIncidentPublisher
{
    // traffic.analytics.incident.{incident_id}
    Task PublishIncidentAsync(TrafficIncidentMessage message);
}