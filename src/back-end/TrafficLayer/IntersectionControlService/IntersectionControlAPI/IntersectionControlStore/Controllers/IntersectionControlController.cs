using IntersectionControlStore.Business;
using IntersectionControlStore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntersectionControlStore.Controllers
{
    [ApiController]
    [Route("intersections/{intersectionId:guid}/priority")]
    public class IntersectionControlController : ControllerBase
{
        private readonly IPriorityManager _priorityManager;

        public IntersectionControlController(IPriorityManager priorityManager)
        {
            _priorityManager = priorityManager;
        }

        [HttpGet]
        //[Authorize(Roles = "Operator,Admin")]
        public ActionResult<PriorityStatusResponse> GetPriorityStatus(Guid intersectionId)
        {
            var status = _priorityManager.GetPriorityStatus(intersectionId);
            if (status == null)
                return NotFound();

            var response = new PriorityStatusResponse
            {
                IntersectionId = status.IntersectionId,
                PriorityEmergencyVehicle = status.PriorityEmergencyVehicle,
                PriorityPublicTransport = status.PriorityPublicTransport,
                PriorityPedestrian = status.PriorityPedestrian,
                PriorityCyclist = status.PriorityCyclist,
                UpdatedAt = status.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost("override")]
        //[Authorize(Roles = "Operator,Admin")]
        public async Task<IActionResult> OverridePriority(Guid intersectionId, [FromBody] PriorityOverrideRequest request)
        {
            var overrideStatus = new IntersectionPriorityStatus
            {
                IntersectionId = intersectionId,
                PriorityEmergencyVehicle = request.PriorityEmergencyVehicle,
                PriorityPublicTransport = request.PriorityPublicTransport,
                PriorityPedestrian = request.PriorityPedestrian,
                PriorityCyclist = request.PriorityCyclist,
                UpdatedAt = DateTime.UtcNow
            };

            await _priorityManager.OverridePriorityAsync(intersectionId, overrideStatus, request.Duration);

            return Ok(new
            {
                success = true,
                message = $"Priorities overridden for {request.Duration} seconds."
            });
        }
    }
}
