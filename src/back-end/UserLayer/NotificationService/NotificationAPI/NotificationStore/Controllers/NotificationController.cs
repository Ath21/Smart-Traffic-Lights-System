/*
 * NotificationStore.Controllers.NotificationController
 *
 * This file is part of the NotificationStore project, which defines the NotificationController class.
 * The NotificationController class is an ASP.NET Core API controller that handles HTTP requests related to notifications.
 * It provides endpoints for sending notifications, retrieving all notifications, and getting notifications by recipient email.
 * The controller uses the INotificationService to encapsulate the business logic for notifications,
 * allowing for separation of concerns between the API layer and the business logic layer.
 * It includes these endpoints:
 * - POST: /API/Notification/SendNotification: Sends a notification to a recipient.
 * - GET: /API/Notification/GetAllNotifications: Retrieves all notifications.
 * - GET: /API/Notification/GetNotificationsByRecipientEmail?recipientEmail={recipientEmail}: Retrieves notifications for a specific recipient email.
 */  
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

        // POST: API/Notification/SendNotification
        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto notification)
        {
            if (string.IsNullOrWhiteSpace(notification.RecipientEmail) ||
                string.IsNullOrWhiteSpace(notification.Message) ||
                string.IsNullOrWhiteSpace(notification.Type))
            {
                return BadRequest("Type, Message, and RecipientEmail are required fields.");
            }

            await _notificationService.SendNotificationAsync(notification);

            return Ok(new { status = "sent", recipient = notification.RecipientEmail });
        }

        // GET: API/Notification/GetAllNotifications
        [HttpGet("GetAllNotifications")]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _notificationService.GetAllNotificationsAsync();
            return Ok(notifications);
        }

        // GET: API/Notification/GetNotificationsByRecipientEmail?recipientEmail={recipientEmail}
        [HttpGet("GetNotificationsByRecipientEmail")]
        public async Task<IActionResult> GetByRecipientEmail([FromQuery] string recipientEmail)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                return BadRequest("RecipientEmail is required.");
            }

            var notifications = await _notificationService.GetNotificationsByRecipientEmailAsync(recipientEmail);
            return Ok(notifications);
        }
        
    }
}
