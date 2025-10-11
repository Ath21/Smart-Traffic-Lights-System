using System;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore.Business;

public interface ISensorBusiness
{
    Task<VehicleCountResponse> RecordVehicleCountAsync(VehicleCountRequest request);
    Task<IEnumerable<VehicleCountResponse>> GetRecentVehicleCountsAsync(int intersectionId);

    Task<PedestrianCountResponse> RecordPedestrianCountAsync(PedestrianCountRequest request);
    Task<IEnumerable<PedestrianCountResponse>> GetRecentPedestrianCountsAsync(int intersectionId);

    Task<CyclistCountResponse> RecordCyclistCountAsync(CyclistCountRequest request);
    Task<IEnumerable<CyclistCountResponse>> GetRecentCyclistCountsAsync(int intersectionId);
}
