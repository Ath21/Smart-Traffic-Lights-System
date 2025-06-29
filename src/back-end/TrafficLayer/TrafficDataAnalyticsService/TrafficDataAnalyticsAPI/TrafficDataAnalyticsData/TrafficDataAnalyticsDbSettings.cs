namespace TrafficDataAnalyticsData;

public class TrafficDataAnalyticsDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;  
    public string IntersectionsCollectionName { get; set; } = null!;
    public string VehicleCountsCollectionName { get; set; } = null!;
    public string PedestrianCountsCollectionName { get; set; } = null!;
    public string CyclistCountsCollectionName { get; set; } = null!;
    public string CongestionAlertsCollectionName { get; set; } = null!;
    public string DailySummaryCollectionName { get; set; } = null!;
}

