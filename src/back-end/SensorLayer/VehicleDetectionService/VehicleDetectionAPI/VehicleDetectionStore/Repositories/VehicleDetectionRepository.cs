using System;
using DetectionData;
using DetectionData.TimeSeriesObjects;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace VehicleDetectionStore.Repositories;

public class VehicleDetectionRepository : IVehicleDetectionRepository
{
    private readonly DetectionDbContext _context;

    public VehicleDetectionRepository(DetectionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Inserts a new vehicle detection record into InfluxDB
    /// </summary>
    public async Task<Guid> InsertAsync(VehicleDetection detection)
    {
        detection.DetectionId = Guid.NewGuid();

        var point = PointData
            .Measurement("vehicle_detections")
            .Tag("intersection_id", detection.IntersectionId.ToString())
            .Field("vehicle_count", detection.VehicleCount)
            .Field("avg_speed", detection.AvgSpeed)
            .Field("detection_id", detection.DetectionId.ToString())
            .Timestamp(detection.Timestamp, WritePrecision.Ns);

        var writeApi = _context.Client.GetWriteApiAsync();
        await writeApi.WritePointAsync(point, _context.Bucket, _context.Org);

        return detection.DetectionId;
    }

    public async Task<List<VehicleDetection>> QueryAsync(
        Guid? intersectionId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? limit = null)
    {
        DateTime start = startTime ?? DateTime.UtcNow.AddDays(-1);
        DateTime end = endTime ?? DateTime.UtcNow;

        var flux = $"from(bucket: \"{_context.Bucket}\") |> range(start: {FormatDate(start)}, stop: {FormatDate(end)}) |> filter(fn: (r) => r._measurement == \"vehicle_detections\")";

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

        var results = new List<VehicleDetection>();

        foreach (var table in tables)
        {
            Console.WriteLine($"Records in table: {table.Records.Count}");
            foreach (var record in table.Records)
            {
                // We're interested only in the actual data field rows (_field and _value)
                if (record.GetValue() != null && record.GetField() != null)
                {
                    // Find or create VehicleDetection object for this timestamp + intersection
                    var existing = results.Find(d =>
                        d.Timestamp == record.GetTimeInDateTime().Value &&
                        d.IntersectionId.ToString() == record.Values["intersection_id"].ToString());

                    if (existing == null)
                    {
                        existing = new VehicleDetection
                        {
                            Timestamp = record.GetTimeInDateTime().Value,
                            IntersectionId = Guid.Parse(record.Values["intersection_id"].ToString())
                        };
                        results.Add(existing);
                    }

                    // Map fields accordingly
                    switch (record.GetField())
                    {
                        case "vehicle_count":
                            existing.VehicleCount = Convert.ToInt32(record.GetValue());
                            break;
                        case "avg_speed":
                            existing.AvgSpeed = Convert.ToSingle(record.GetValue());
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
