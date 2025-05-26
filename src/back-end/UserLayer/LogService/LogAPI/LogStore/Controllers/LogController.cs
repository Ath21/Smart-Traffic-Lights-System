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

        [HttpPost]
        public async Task<IActionResult> PostLog([FromBody] LogDto logDto)
        {
            await _logService.StoreLogAsync(logDto);
            return Ok();  
        }
    }
}
