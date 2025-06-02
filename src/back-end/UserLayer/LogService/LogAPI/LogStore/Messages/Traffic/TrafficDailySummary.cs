namespace LogStore.Messages.Traffic;

public record class TrafficDailySummary(
    string IntersectionId,     // Target intersection or "ALL"
    int VehicleCount,          // Total vehicles recorded
    double AverageSpeed,       // Average speed in km/h
    string Notes,              // Optional extra info or summary line
    DateTime Timestamp         // UTC timestamp
);