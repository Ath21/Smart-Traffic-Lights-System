using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using DetectionData.Collections.Count;
using DetectionData.Collections.Detection;

namespace DetectionData;

public class DetectionDbContext
{
    private readonly IMongoDatabase _database;

    public DetectionDbContext(IOptions<DetectionDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);

        VehicleCounts = _database.GetCollection<VehicleCount>(settings.Value.VehicleCountCollection);
        PedestrianCounts = _database.GetCollection<PedestrianCount>(settings.Value.PedestrianCountCollection);
        CyclistCounts = _database.GetCollection<CyclistCount>(settings.Value.CyclistCountCollection);

        EmergencyVehicles = _database.GetCollection<EmergencyVehicleDetection>(settings.Value.EmergencyVehicleCollection);
        Incidents = _database.GetCollection<IncidentDetection>(settings.Value.IncidentCollection);
        PublicTransport = _database.GetCollection<PublicTransportDetection>(settings.Value.PublicTransportCollection);
    }

    public IMongoCollection<VehicleCount> VehicleCounts { get; }
    public IMongoCollection<PedestrianCount> PedestrianCounts { get; }
    public IMongoCollection<CyclistCount> CyclistCounts { get; }

    public IMongoCollection<EmergencyVehicleDetection> EmergencyVehicles { get; }
    public IMongoCollection<IncidentDetection> Incidents { get; }
    public IMongoCollection<PublicTransportDetection> PublicTransport { get; }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
