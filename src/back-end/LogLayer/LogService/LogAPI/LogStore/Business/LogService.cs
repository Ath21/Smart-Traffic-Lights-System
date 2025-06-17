/*
 * LogStore.Business.LogService
 *
 * This class implements the ILogService interface, providing methods for managing logs.
 * It uses an ILogRepository to interact with the data layer and AutoMapper to map between
 * LogDto and Log models.
 * The methods include storing logs, retrieving all logs, and retrieving logs by service.
 * The methods are asynchronous to support non-blocking operations.
 * The LogDto model is used to transfer log data between the service and the data layer.
 */
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

    // GET: /API/Log/GetAllLogs
    public Task<List<LogDto>> GetAllLogsAsync()
    {
        var logs = _logRepository.GetAllAsync();
        return logs.ContinueWith(task => _mapper.Map<List<LogDto>>(task.Result));
    }

    // GET: /API/Log/GetLogsByService?service=ServiceName
    public Task<List<LogDto>> GetLogsByServiceAsync(string service)
    {
        var logs = _logRepository.GetAsync(service);
        return logs.ContinueWith(task => _mapper.Map<List<LogDto>>(task.Result));
    }

    // POST: /API/Log/CreateLog
    public async Task StoreLogAsync(LogDto logDto)
    {
        var log = _mapper.Map<Log>(logDto);
        await _logRepository.CreateAsync(log);
    }
}
