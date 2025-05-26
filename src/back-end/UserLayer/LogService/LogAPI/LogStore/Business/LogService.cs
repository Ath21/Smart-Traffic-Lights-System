using System;
using AutoMapper;
using LogData.Collections;
using LogStore.Models;
using LogStore.Repository;

namespace LogStore.Business;

public class LogService : ILogService
{
    private readonly ILogRepository _logRepository;
    private readonly IMapper _mapper;

    public LogService(ILogRepository logRepository, IMapper mapper)
    {
        _logRepository = logRepository;
        _mapper = mapper;
    }

    public async Task StoreLogAsync(LogDto logDto)
    {
        var log = _mapper.Map<Log>(logDto);
        await _logRepository.CreateAsync(log); 
    }
}
