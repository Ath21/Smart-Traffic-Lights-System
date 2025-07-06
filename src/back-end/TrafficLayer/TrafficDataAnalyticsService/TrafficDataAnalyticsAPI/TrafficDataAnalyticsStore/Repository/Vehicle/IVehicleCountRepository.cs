using System;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Vehicle;

public interface IVehicleCountRepository
{
    Task<List<VehicleCount>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date);
    Task AddAsync(VehicleCount entry);
}
