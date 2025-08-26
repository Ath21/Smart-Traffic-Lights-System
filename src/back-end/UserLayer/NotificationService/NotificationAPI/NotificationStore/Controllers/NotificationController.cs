using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationStore.Business.Notify;
using NotificationStore.Models;

namespace NotificationStore.Controllers;

[Route("api/notifications")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // POST: /api/notifications/send
    [HttpPost("send")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> SendUserNotification([FromBody] SendNotificationRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Message) ||
            string.IsNullOrWhiteSpace(request.Type) ||
            request.UserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.RecipientEmail))
        {
            return BadRequest("UserId, RecipientEmail, Type, and Message are required.");
        }

        await _notificationService.SendUserNotificationAsync(
            request.UserId,
            request.RecipientEmail,
            request.Message,
            request.Type
        );

        return Ok(new
        {
            status = "sent",
            recipient = request.RecipientEmail,
            type = request.Type
        });
    }


    // POST: /api/notifications/public-notice
    [HttpPost("public-notice")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> SendPublicNotice([FromBody] PublicNoticeRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Message) ||
            string.IsNullOrWhiteSpace(request.TargetAudience))
        {
            return BadRequest("Title, Message, and TargetAudience are required.");
        }

        await _notificationService.SendPublicNoticeAsync(request.Title, request.Message, request.TargetAudience);

        return Ok(new { status = "published", audience = request.TargetAudience });
    }

    // GET: /api/notifications/user/{email}
    [HttpGet("user/{email}")]
    [Authorize(Roles = "User,Admin,Operator")]
    public async Task<IActionResult> GetByRecipientEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        var notifications = await _notificationService.GetNotificationsByRecipientEmailAsync(email);
        return Ok(notifications);
    }

    // GET: /api/notifications/history/{userId}
    [HttpGet("history/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserHistory(Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest("UserId is required.");

        var logs = await _notificationService.GetDeliveryHistoryAsync(userId);
        return Ok(logs);
    }

    // PATCH: /api/notifications/{notificationId}/read
    [HttpPatch("{notificationId}/read")]
    [Authorize(Roles = "User,Admin,Operator")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var userEmail = User.Identity?.Name; // or claim
        if (string.IsNullOrWhiteSpace(userEmail))
            return Unauthorized();

        await _notificationService.MarkAsReadAsync(notificationId, userEmail);
        return Ok(new { status = "read", notificationId });
    }
    
    // PATCH: /api/notifications/read-all
    [HttpPatch("read-all")]
    [Authorize(Roles = "User,Admin,Operator")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userEmail = User.Identity?.Name; // or claim
        if (string.IsNullOrWhiteSpace(userEmail))
            return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(userEmail);
        return Ok(new { status = "all-read" });
    }


}
