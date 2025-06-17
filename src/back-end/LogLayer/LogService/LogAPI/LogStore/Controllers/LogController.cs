/*
 *  LogStore.Controllers.LogController
 *
 *  This controller handles HTTP requests related to log management.
 *  It provides endpoints for creating logs, retrieving all logs, and retrieving logs by service.
 *  The controller uses the ILogService to perform operations on logs.
 *  The endpoints are designed to be used by clients to log messages and retrieve log data.
 *
 *  Endpoints:
 *  - POST API/Log/CreateLog
 *      Accepts a LogDto object in the request body and stores it using the log service.
 *  - GET API/Log/GetAllLogs
 *      Returns a list of all logs stored in the system.
 *  - GET API/Log/GetLogsByService/{service}
 *      Returns a list of logs filtered by the specified service name.
 */
using LogStore.Business;
using LogStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogStore.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        // POST: API/Log/CreateLog
        [HttpPost("CreateLog")]
        public async Task<IActionResult> PostLog([FromBody] LogDto logDto)
        {
            await _logService.StoreLogAsync(logDto);
            return Ok();
        }

        // GET: API/Log/GetAllLogs
        [HttpGet("GetAllLogs")]
        public async Task<ActionResult<List<LogDto>>> GetAllLogs()
        {
            var logs = await _logService.GetAllLogsAsync();
            return Ok(logs);
        }

        // GET: API/Log/GetLogsByService/{service}
        [HttpGet("GetLogsByService/{service}")]
        public async Task<ActionResult<List<LogDto>>> GetLogsByService(string service)
        {
            var logs = await _logService.GetLogsByServiceAsync(service);
            if (logs == null || logs.Count == 0)
            {
                return NotFound($"No logs found for service: {service}");
            }
            return Ok(logs);
        }
    }
}
