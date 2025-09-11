using System;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Light;

public interface ILightRepository
{
    Task<List<TrafficLight>> GetByIntersectionAsync(Guid intersectionId, CancellationToken ct);
    Task<TrafficLight?> GetLatestAsync(Guid intersectionId, CancellationToken ct);
}
