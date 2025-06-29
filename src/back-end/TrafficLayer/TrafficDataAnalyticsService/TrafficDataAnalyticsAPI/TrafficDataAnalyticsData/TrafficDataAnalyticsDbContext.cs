
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrafficDataAnalyticsData.Collections;

namespace TrafficDataAnalyticsData;

public class TrafficDataAnalyticsDbContext
{
    private readonly IMongoCollection<Intersection> _intersectionsCollection;
    private readonly IMongoCollection<VehicleCount> _vehicleCountsCollection;
    private readonly IMongoCollection<PedestrianCount> _pedestrianCountsCollection;
    private readonly IMongoCollection<CyclistCount> _cyclistCountsCollection;
    private readonly IMongoCollection<DailySummary> _dailySummariesCollection;
    private readonly IMongoCollection<CongestionAlert> _congestionAlertsCollection;


    public TrafficDataAnalyticsDbContext(IOptions<TrafficDataAnalyticsDbSettings> trafficDataAnalyticsDbSettings)
    {
        var mongoClient = new MongoClient(
            trafficDataAnalyticsDbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            trafficDataAnalyticsDbSettings.Value.DatabaseName);

        _intersectionsCollection = mongoDatabase.GetCollection<Intersection>(
            trafficDataAnalyticsDbSettings.Value.IntersectionsCollectionName);
        _vehicleCountsCollection = mongoDatabase.GetCollection<VehicleCount>(
            trafficDataAnalyticsDbSettings.Value.VehicleCountsCollectionName);
        _pedestrianCountsCollection = mongoDatabase.GetCollection<PedestrianCount>(
            trafficDataAnalyticsDbSettings.Value.PedestrianCountsCollectionName);
        _cyclistCountsCollection = mongoDatabase.GetCollection<CyclistCount>(
            trafficDataAnalyticsDbSettings.Value.CyclistCountsCollectionName);
        _congestionAlertsCollection = mongoDatabase.GetCollection<CongestionAlert>(
            trafficDataAnalyticsDbSettings.Value.CongestionAlertsCollectionName);
        _dailySummariesCollection = mongoDatabase.GetCollection<DailySummary>(
            trafficDataAnalyticsDbSettings.Value.DailySummaryCollectionName);
    }

    public IMongoCollection<Intersection> IntersectionsCollection => _intersectionsCollection;
    public IMongoCollection<VehicleCount> VehicleCountsCollection => _vehicleCountsCollection;
    public IMongoCollection<PedestrianCount> PedestrianCountsCollection => _pedestrianCountsCollection;
    public IMongoCollection<CyclistCount> CyclistCountsCollection => _cyclistCountsCollection;
    public IMongoCollection<CongestionAlert> CongestionAlertsCollection => _congestionAlertsCollection;
    public IMongoCollection<DailySummary> DailySummariesCollection => _dailySummariesCollection;
}
