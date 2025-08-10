using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DetectionData;
using DetectionData.TimeSeriesObjects;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace IncidentDetectionStore.Repositories
{
    public class IncidentDetectionRepository : IIncidentDetectionRepository
    {
        private readonly DetectionDbContext _context;

        public IncidentDetectionRepository(DetectionDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Inserts a new incident detection record into InfluxDB
        /// </summary>
        public async Task<Guid> InsertAsync(IncidentDetection detection)
        {
            detection.DetectionId = Guid.NewGuid();

            var point = PointData
                .Measurement("incident_detections")
                .Tag("intersection_id", detection.IntersectionId.ToString())
                .Field("description", detection.Description)
                .Field("detection_id", detection.DetectionId.ToString())
                .Timestamp(detection.Timestamp, WritePrecision.Ns);

            var writeApi = _context.Client.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, _context.Bucket, _context.Org);

            return detection.DetectionId;
        }

        /// <summary>
        /// Queries incident detections from InfluxDB
        /// </summary>
        public async Task<List<IncidentDetection>> QueryAsync(
            Guid? intersectionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null)
        {
            DateTime start = startTime ?? DateTime.UtcNow.AddDays(-1);
            DateTime end = endTime ?? DateTime.UtcNow;

            var flux = $"from(bucket: \"{_context.Bucket}\")" +
                       $" |> range(start: {FormatDate(start)}, stop: {FormatDate(end)})" +
                       $" |> filter(fn: (r) => r._measurement == \"incident_detections\")";

            if (intersectionId.HasValue)
            {
                flux += $" |> filter(fn: (r) => r.intersection_id == \"{intersectionId.Value}\")";
            }

            if (limit.HasValue)
            {
                flux += $" |> limit(n: {limit.Value})";
            }

            var queryApi = _context.Client.GetQueryApi();
            var tables = await queryApi.QueryAsync(flux, _context.Org);

            var results = new List<IncidentDetection>();

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    if (record.GetValue() != null && record.GetField() != null)
                    {
                        var existing = results.Find(d =>
                            d.Timestamp == record.GetTimeInDateTime().Value &&
                            d.IntersectionId.ToString() == record.Values["intersection_id"].ToString());

                        if (existing == null)
                        {
                            existing = new IncidentDetection
                            {
                                Timestamp = record.GetTimeInDateTime().Value,
                                IntersectionId = Guid.Parse(record.Values["intersection_id"].ToString())
                            };
                            results.Add(existing);
                        }

                        switch (record.GetField())
                        {
                            case "description":
                                existing.Description = record.GetValue().ToString();
                                break;
                            case "detection_id":
                                existing.DetectionId = Guid.Parse(record.GetValue().ToString());
                                break;
                        }
                    }
                }
            }

            return results;
        }

        private string FormatDate(DateTime dateTime) =>
            dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
