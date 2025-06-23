using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationStore.Business.Notify;
using NotificationStore.Models;

namespace NotificationStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // POST: API/Notification/Send
        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] NotificationDto notification)
        {
            if (string.IsNullOrWhiteSpace(notification.RecipientEmail) ||
                string.IsNullOrWhiteSpace(notification.Message) ||
                string.IsNullOrWhiteSpace(notification.Type))
            {
                return BadRequest("Type, Message, and RecipientEmail are required fields.");
            }

            await _notificationService.CreateAsync(notification);

            return Ok(new { status = "sent", recipient = notification.RecipientEmail });
        }

        // GET: API/Notification/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _notificationService.GetAllAsync();
            return Ok(notifications);
        }

        // GET: API/Notification/GetByRecipientEmail
        [HttpGet("GetByRecipientEmail")]
        public async Task<IActionResult> GetByRecipientEmail([FromQuery] string recipientEmail)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                return BadRequest("RecipientEmail is required.");
            }

            var notifications = await _notificationService.GetByRecipientEmailAsync(recipientEmail);
            return Ok(notifications);
        }
        
    }
}
