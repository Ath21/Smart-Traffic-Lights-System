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
    }

    public IMongoCollection<VehicleCount> VehicleCounts => 
        _database.GetCollection<VehicleCount>("vehicle_count");

    public IMongoCollection<PedestrianCount> PedestrianCounts => 
        _database.GetCollection<PedestrianCount>("pedestrian_count");

    public IMongoCollection<CyclistCount> CyclistCounts => 
        _database.GetCollection<CyclistCount>("cyclist_count");

    public IMongoCollection<EmergencyVehicleDetection> EmergencyVehicles => 
        _database.GetCollection<EmergencyVehicleDetection>("emergency_vehicle");

    public IMongoCollection<PublicTransportDetection> PublicTransports => 
        _database.GetCollection<PublicTransportDetection>("public_transport");

    public IMongoCollection<IncidentDetection> Incidents => 
        _database.GetCollection<IncidentDetection>("incident");
}