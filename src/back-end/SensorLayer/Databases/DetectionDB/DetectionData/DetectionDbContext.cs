using DetectionData.Collection.Count;
using DetectionData.Collection.Detection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DetectionData;

public class DetectionDbContext
{
    private readonly IMongoDatabase _database;

    public DetectionDbContext(IOptions<DetectionDbSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        _database = mongoClient.GetDatabase(settings.Value.Database);

        VehicleCounts = _database.GetCollection<VehicleCount>(settings.Value.VehicleCollection);
        PedestrianCounts = _database.GetCollection<PedestrianCount>(settings.Value.PedestrianCollection);
        CyclistCounts = _database.GetCollection<CyclistCount>(settings.Value.CyclistCollection);
        EmergencyVehicles = _database.GetCollection<EmergencyVehicleDetection>(settings.Value.EmergencyCollection);
        PublicTransports = _database.GetCollection<PublicTransportDetection>(settings.Value.PublicTransportCollection);
        Incidents = _database.GetCollection<IncidentDetection>(settings.Value.IncidentCollection);
    }

    public IMongoCollection<VehicleCount> VehicleCounts { get; }
    public IMongoCollection<PedestrianCount> PedestrianCounts { get; }
    public IMongoCollection<CyclistCount> CyclistCounts { get; }
    public IMongoCollection<EmergencyVehicleDetection> EmergencyVehicles { get; }
    public IMongoCollection<PublicTransportDetection> PublicTransports { get; }
    public IMongoCollection<IncidentDetection> Incidents { get; }
}
