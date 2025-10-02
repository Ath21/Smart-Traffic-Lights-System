using System;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Light;

public interface ITrafficLightRepository : IRepository<TrafficLight>
{
    Task<IEnumerable<TrafficLight>> GetByIntersectionAsync(int intersectionId);
    Task<IEnumerable<TrafficLight>> GetByStateAsync(TrafficLightState state);
}

