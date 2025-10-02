using TrafficLightData.Entities;

namespace TrafficLightData.Repositories.Intersections;

public interface IIntersectionRepository : IRepository<Intersection>
{
    Task<Intersection?> GetWithDetailsAsync(int id);
}
