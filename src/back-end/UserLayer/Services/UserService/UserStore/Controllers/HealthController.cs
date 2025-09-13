using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace UserStore.Controllers
{
    [ApiController]
    [Route("user_service")]
    public class HealthController : ControllerBase
    {
        // Liveness: Service is running (no external deps)
        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", service = "User Service" });
        }
    }
}