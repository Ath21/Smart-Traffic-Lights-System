using System;
using Messages.Sensor;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore.Business;

public interface ISensorBusiness
{
    Task<IEnumerable<VehicleCountResponse>> GetRecentVehicleCountsAsync(int intersectionId);
    Task<IEnumerable<PedestrianCountResponse>> GetRecentPedestrianCountsAsync(int intersectionId);
    Task<IEnumerable<CyclistCountResponse>> GetRecentCyclistCountsAsync(int intersectionId);
    Task ProcessSensorAsync(SensorCountMessage sensorMsg);
}
