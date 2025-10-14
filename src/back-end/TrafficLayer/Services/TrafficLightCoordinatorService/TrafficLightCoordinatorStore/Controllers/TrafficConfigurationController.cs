using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficLightData;

namespace TrafficLightCoordinatorStore.Controllers;

[ApiController]
[Route("api/configurations")]
public class TrafficConfigurationsController : ControllerBase
{
    private readonly TrafficLightDbContext _db;
    private readonly ILogger<TrafficConfigurationsController> _logger;

    public TrafficConfigurationsController(TrafficLightDbContext db, ILogger<TrafficConfigurationsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /api/configurations
    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll()
    {
        var configs = await _db.TrafficConfigurations
            .AsNoTracking()
            .OrderBy(c => c.ConfigurationId)
            .Select(c => new
            {
                c.ConfigurationId,
                c.Mode,
                c.CycleDurationSec,
                c.GlobalOffsetSec,
                PhaseDurations = c.PhaseDurationsJson,
                c.Purpose,
                c.LastUpdated
            })
            .ToListAsync();

        return Ok(configs);
    }

    // GET: /api/configurations/{mode}
    [HttpGet]
    [Route("mode")]
    public async Task<IActionResult> GetByMode(string mode)
    {
        var config = await _db.TrafficConfigurations
            .AsNoTracking()
            .Where(c => c.Mode.ToLower() == mode.ToLower())
            .Select(c => new
            {
                c.ConfigurationId,
                c.Mode,
                c.CycleDurationSec,
                c.GlobalOffsetSec,
                PhaseDurations = c.PhaseDurationsJson,
                c.Purpose,
                c.LastUpdated
            })
            .FirstOrDefaultAsync();

        if (config is null)
            return NotFound(new { error = $"Mode '{mode}' not found" });

        return Ok(config);
    }
}
