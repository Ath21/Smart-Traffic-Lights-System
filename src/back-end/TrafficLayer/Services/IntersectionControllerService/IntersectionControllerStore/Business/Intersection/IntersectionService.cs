using System;
using AutoMapper;
using IntersectionControllerStore.Models.Dtos;
using TrafficLightCacheData.Repositories.Intersect;


namespace IntersectionControllerStore.Business.Intersection;

public class IntersectionService : IIntersectionService
{
    private readonly IIntersectRepository _repo;
    private readonly IMapper _mapper;

    public IntersectionService(IIntersectRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IntersectionDto?> GetIntersectionAsync(Guid intersectionId)
    {
        var entity = await _repo.GetAsync(intersectionId);
        return entity != null ? _mapper.Map<IntersectionDto>(entity) : null;
    }

    public async Task<IEnumerable<IntersectionDto>> GetAllAsync()
    {
        // Redis does not support listing easily → optional if you plan to cache a full list
        return new List<IntersectionDto>();
    }
}
