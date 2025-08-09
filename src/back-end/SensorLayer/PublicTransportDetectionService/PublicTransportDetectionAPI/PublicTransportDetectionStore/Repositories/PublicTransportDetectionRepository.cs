using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DetectionData;
using DetectionData.TimeSeriesObjects;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace PublicTransportDetectionStore.Repositories
{
    public class PublicTransportDetectionRepository : IPublicTransportDetectionRepository
    {
        private readonly DetectionDbContext _context;

        public PublicTransportDetectionRepository(DetectionDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Inserts a new public transport detection record into InfluxDB
        /// </summary>
        public async Task<Guid> InsertAsync(PublicTransportDetection detection)
        {
            detection.DetectionId = Guid.NewGuid();

            var point = PointData
                .Measurement("public_transport_detections")
                .Tag("intersection_id", detection.IntersectionId.ToString())
                .Tag("route_id", detection.RouteId)
                .Field("detection_id", detection.DetectionId.ToString())
                .Timestamp(detection.Timestamp, WritePrecision.Ns);

            var writeApi = _context.Client.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, _context.Bucket, _context.Org);

            return detection.DetectionId;
        }

        /// <summary>
        /// Queries public transport detections from InfluxDB
        /// </summary>
        public async Task<List<PublicTransportDetection>> QueryAsync(
            Guid? intersectionId = null,
            string? routeId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null)
        {
            DateTime start = startTime ?? DateTime.UtcNow.AddDays(-1);
            DateTime end = endTime ?? DateTime.UtcNow;

            var flux = $"from(bucket: \"{_context.Bucket}\")" +
                       $" |> range(start: {FormatDate(start)}, stop: {FormatDate(end)})" +
                       $" |> filter(fn: (r) => r._measurement == \"public_transport_detections\")";

            if (intersectionId.HasValue)
                flux += $" |> filter(fn: (r) => r.intersection_id == \"{intersectionId.Value}\")";

            if (!string.IsNullOrEmpty(routeId))
                flux += $" |> filter(fn: (r) => r.route_id == \"{routeId}\")";

            if (limit.HasValue)
                flux += $" |> limit(n: {limit.Value})";

            Console.WriteLine($"Executing Flux query: {flux}");

            var queryApi = _context.Client.GetQueryApi();
            var tables = await queryApi.QueryAsync(flux, _context.Org);

            var results = new List<PublicTransportDetection>();

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
                            existing = new PublicTransportDetection
                            {
                                Timestamp = record.GetTimeInDateTime().Value,
                                IntersectionId = Guid.Parse(record.Values["intersection_id"].ToString()),
                                RouteId = record.Values["route_id"].ToString()
                            };
                            results.Add(existing);
                        }

                        if (record.GetField() == "detection_id")
                            existing.DetectionId = Guid.Parse(record.GetValue().ToString());
                    }
                }
            }

            return results;
        }

        private string FormatDate(DateTime dateTime) =>
            dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
