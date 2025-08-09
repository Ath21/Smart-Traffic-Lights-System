using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DetectionData;
using DetectionData.TimeSeriesObjects;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace EmergencyVehicleDetectionStore.Repositories
{
    public class EmergencyVehicleDetectionRepository : IEmergencyVehicleDetectionRepository
    {
        private readonly DetectionDbContext _context;

        public EmergencyVehicleDetectionRepository(DetectionDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Inserts a new emergency vehicle detection record into InfluxDB
        /// </summary>
        public async Task<Guid> InsertAsync(EmergencyVehicleDetection detection)
        {
            detection.DetectionId = Guid.NewGuid();

            var point = PointData
                .Measurement("emergency_vehicle_detections")  // changed measurement name
                .Tag("intersection_id", detection.IntersectionId.ToString())
                .Field("detected", detection.Detected) // store boolean field
                .Field("detection_id", detection.DetectionId.ToString())
                .Timestamp(detection.Timestamp, WritePrecision.Ns);

            var writeApi = _context.Client.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, _context.Bucket, _context.Org);

            return detection.DetectionId;
        }

        /// <summary>
        /// Queries emergency vehicle detections from InfluxDB
        /// </summary>
        public async Task<List<EmergencyVehicleDetection>> QueryAsync(
            Guid? intersectionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null)
        {
            DateTime start = startTime ?? DateTime.UtcNow.AddDays(-1);
            DateTime end = endTime ?? DateTime.UtcNow;

            var flux = $"from(bucket: \"{_context.Bucket}\")" +
                       $" |> range(start: {FormatDate(start)}, stop: {FormatDate(end)})" +
                       $" |> filter(fn: (r) => r._measurement == \"emergency_vehicle_detections\")";

            if (intersectionId.HasValue)
            {
                flux += $" |> filter(fn: (r) => r.intersection_id == \"{intersectionId.Value}\")";
            }

            if (limit.HasValue)
            {
                flux += $" |> limit(n: {limit.Value})";
            }

            Console.WriteLine($"Executing Flux query: {flux}");

            var queryApi = _context.Client.GetQueryApi();
            var tables = await queryApi.QueryAsync(flux, _context.Org);

            Console.WriteLine($"Tables returned: {tables.Count}");

            var results = new List<EmergencyVehicleDetection>();

            foreach (var table in tables)
            {
                Console.WriteLine($"Records in table: {table.Records.Count}");
                foreach (var record in table.Records)
                {
                    if (record.GetValue() != null && record.GetField() != null)
                    {
                        var existing = results.Find(d =>
                            d.Timestamp == record.GetTimeInDateTime().Value &&
                            d.IntersectionId.ToString() == record.Values["intersection_id"].ToString());

                        if (existing == null)
                        {
                            existing = new EmergencyVehicleDetection
                            {
                                Timestamp = record.GetTimeInDateTime().Value,
                                IntersectionId = Guid.Parse(record.Values["intersection_id"].ToString())
                            };
                            results.Add(existing);
                        }

                        switch (record.GetField())
                        {
                            case "detected":
                                existing.Detected = Convert.ToBoolean(record.GetValue());
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
