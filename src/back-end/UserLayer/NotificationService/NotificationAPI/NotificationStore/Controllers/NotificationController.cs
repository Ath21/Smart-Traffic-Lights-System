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

        // GET: API/Notification/GetByRecipient?recipientId=Guid
        [HttpGet("GetByRecipient/{recipientId:guid}")]
        public async Task<IActionResult> GetByRecipient(Guid recipientId)
        {
            var notifications = await _notificationService.GetByRecipientAsync(recipientId);
            if (notifications == null || notifications.Count == 0)
            {
                return NotFound($"No notifications found for recipient: {recipientId}");
            }
            return Ok(notifications);
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
        
    }
}
