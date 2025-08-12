using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrafficLightControlStore.Business;
using TrafficLightControlStore.Models;

namespace TrafficLightControlService.Controllers
{
    [ApiController]
    [Route("traffic-lights")]
    public class TrafficLightsController : ControllerBase
    {
        private readonly ITrafficLightManager _manager;

        public TrafficLightsController(ITrafficLightManager manager)
        {
            _manager = manager;
        }

        [HttpPost("{light_id:guid}/override")]
        //[Authorize(Roles = "Operator,Admin")]
        public async Task<IActionResult> OverrideLight(Guid light_id, [FromBody] TrafficLightOverrideRequest request)
        {
            await _manager.OverrideLightAsync(light_id, request.State, request.Duration, "ManualOverride");
            return Ok(new
            {
                success = true,
                message = $"Traffic light overridden for {request.Duration} seconds."
            });
        }

        [HttpGet("{light_id:guid}/state")]
        //[Authorize(Roles = "Operator,Admin,Viewer")]
        public IActionResult GetLightState(Guid light_id)
        {
            var state = _manager.GetLightState(light_id);
            if (state == null)
                return NotFound(new { message = "Light not found" });

            return Ok(new
            {
                light_id,
                current_state = state.Value.State,
                updated_at = state.Value.UpdatedAt
            });
        }
    }
}
