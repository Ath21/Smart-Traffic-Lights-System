using System;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Vehicle;

public interface IVehicleCountRepository
{
    Task<List<VehicleCount>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date);
    Task<List<string>> GetAllIntersectionIdsAsync();
    Task<List<VehicleCount>> GetRangeByIntersectionAsync(string intersectionId, DateTime from, DateTime to);
    Task AddAsync(VehicleCount entry);
}
