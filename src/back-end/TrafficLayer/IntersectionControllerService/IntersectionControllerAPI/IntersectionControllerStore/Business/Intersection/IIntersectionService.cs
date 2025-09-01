using System;
using IntersectionControllerStore.Models.Dtos;

namespace IntersectionControllerStore.Business.Intersection;

public interface IIntersectionService
{
    Task<IntersectionDto?> GetIntersectionAsync(Guid intersectionId);
    Task<IEnumerable<IntersectionDto>> GetAllAsync();
}
