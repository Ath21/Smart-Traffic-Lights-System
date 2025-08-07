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

    /// <summary>
    /// Queries vehicle detections based on filters
    /// </summary>
    public async Task<List<VehicleDetection>> QueryAsync(
        Guid? intersectionId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? limit = null)
    {
        var flux = $"from(bucket: \"{_context.Bucket}\") |> range(start: {FormatDate(startTime ?? DateTime.MinValue)}, stop: {FormatDate(endTime ?? DateTime.UtcNow)}) |> filter(fn: (r) => r._measurement == \"vehicle_detections\")";

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

        var results = new List<VehicleDetection>();
        foreach (var table in tables)
        {
            var grouped = table.Records
                .Where(r => r.Values.ContainsKey("detection_id"))
                .GroupBy(r => r.Values["detection_id"])
                .Select(group =>
                {
                    var detection = new VehicleDetection
                    {
                        DetectionId = Guid.Parse(group.Key.ToString()!),
                        IntersectionId = Guid.Parse(group.First().GetValueByKey("intersection_id")!.ToString()!),
                        Timestamp = group.First().GetTimeInDateTime()!.Value
                    };

                    foreach (var record in group)
                    {
                        switch (record.GetField())
                        {
                            case "vehicle_count":
                                detection.VehicleCount = Convert.ToInt32(record.GetValue());
                                break;
                            case "avg_speed":
                                detection.AvgSpeed = Convert.ToSingle(record.GetValue());
                                break;
                        }
                    }
                    return detection;
                });

            results.AddRange(grouped);
        }


        return results;
    }

    private string FormatDate(DateTime dateTime) =>
        dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
}
