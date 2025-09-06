using System;
using AutoMapper;
using IntersectionControllerStore.Models.Dtos;
using IntersectionControllerStore.Repository.Config;

namespace IntersectionControllerStore.Business.TrafficConfig;

public class TrafficConfigurationService : ITrafficConfigurationService
{
    private readonly ITrafficConfigurationRepository _repo;
    private readonly IMapper _mapper;

    public TrafficConfigurationService(ITrafficConfigurationRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<TrafficConfigurationDto?> GetConfigurationAsync(Guid configId)
    {
        var entity = await _repo.GetAsync(configId);
        return entity != null ? _mapper.Map<TrafficConfigurationDto>(entity) : null;
    }

    public async Task<IEnumerable<TrafficConfigurationDto>> GetByIntersectionAsync(Guid intersectionId)
    {
        // For Redis, store configs as a list if needed
        return new List<TrafficConfigurationDto>();
    }
}
