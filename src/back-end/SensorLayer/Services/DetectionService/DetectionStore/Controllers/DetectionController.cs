using Microsoft.AspNetCore.Mvc;
using DetectionStore.Business;
using DetectionStore.Domain;

namespace DetectionStore.Controllers;

[ApiController]
[Route("api/detections")]
public class DetectionController : ControllerBase
{
    private readonly IDetectionBusiness _business;
    private readonly ILogger<DetectionController> _logger;
    private readonly IntersectionContext _intersection;

    public DetectionController(
        IDetectionBusiness business,
        ILogger<DetectionController> logger,
        IntersectionContext intersection)
    {
        _business = business;
        _logger = logger;
        _intersection = intersection;
    }

    [HttpGet]
    [Route("emergency")]
    public async Task<IActionResult> GetRecentEmergencies()
    {
        var data = await _business.GetRecentEmergenciesAsync(_intersection.Id);
        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER][DETECTION] No emergency detections found for {Intersection}", _intersection.Name);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER][DETECTION] {Count} emergency detections returned for {Intersection}", data.Count(), _intersection.Name);
        return Ok(data);
    }

    [HttpGet]
    [Route("public-transport")]
    public async Task<IActionResult> GetPublicTransports()
    {
        var data = await _business.GetRecentPublicTransportsAsync(_intersection.Id);
        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER][DETECTION] No public transport detections found for {Intersection}", _intersection.Name);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER][DETECTION] {Count} public transport detections returned for {Intersection}", data.Count(), _intersection.Name);
        return Ok(data);
    }

    [HttpGet]
    [Route("incident")]
    public async Task<IActionResult> GetRecentIncidents()
    {
        var data = await _business.GetRecentIncidentsAsync(_intersection.Id);
        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER][DETECTION] No incident detections found for {Intersection}", _intersection.Name);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER][DETECTION] {Count} incident detections returned for {Intersection}", data.Count(), _intersection.Name);
        return Ok(data);
    }
}
