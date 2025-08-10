using System;
using System.Threading.Tasks;

namespace IncidentDetectionStore.Publishers
{
    public interface IIncidentDetectionPublisher
    {
        Task PublishIncidentReportAsync(
            Guid intersectionId,
            string incidentType,
            string severity,
            string description,
            DateTime timestamp
        );

        Task PublishAuditLogAsync(string message);
        Task PublishErrorLogAsync(string message, Exception exception);
    }
}
