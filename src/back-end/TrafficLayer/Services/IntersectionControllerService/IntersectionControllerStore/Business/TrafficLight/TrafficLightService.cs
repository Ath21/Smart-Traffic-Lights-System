using System;
using AutoMapper;
using IntersectionControllerStore.Models.Dtos;
using TrafficLightCacheData.Repositories.Light;

namespace IntersectionControllerStore.Business.TrafficLight;

public class TrafficLightService : ITrafficLightService
{
    private readonly ITrafficLightRepository _repo;
    private readonly IMapper _mapper;

    public TrafficLightService(ITrafficLightRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<TrafficLightDto?> GetLightAsync(Guid lightId)
    {
        var entity = await _repo.GetAsync(lightId);
        return entity != null ? _mapper.Map<TrafficLightDto>(entity) : null;
    }

    public async Task<IEnumerable<TrafficLightDto>> GetByIntersectionAsync(Guid intersectionId)
    {
        var states = await _repo.GetLightStatesAsync(intersectionId);
        return states.Select(s => new TrafficLightDto
        {
            IntersectionId = intersectionId,
            LightId = Guid.Parse(s.Key),
            CurrentState = s.Value,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task UpdateLightStateAsync(Guid intersectionId, Guid lightId, string newState)
    {
        await _repo.SetLightStateAsync(intersectionId, lightId, newState);
    }
}