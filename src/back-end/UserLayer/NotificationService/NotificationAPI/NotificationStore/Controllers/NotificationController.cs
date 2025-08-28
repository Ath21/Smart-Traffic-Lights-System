using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationStore.Business.Notify;
using NotificationStore.Models.Requests;
using NotificationStore.Models.Responses;

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
    [Authorize(Roles = "Admin,TrafficOperator")]
    public async Task<ActionResult<NotificationResponse>> SendUserNotification([FromBody] SendNotificationRequest request)
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

        return Ok(new NotificationResponse
        {
            NotificationId = Guid.NewGuid(), 
            Type = request.Type,
            Title = $"{request.Type} Notification",
            Message = request.Message,
            RecipientEmail = request.RecipientEmail,
            Status = "Sent",
            CreatedAt = DateTime.UtcNow
        });
    }

    // POST: /api/notifications/public-notice
    [HttpPost("public-notice")]
    [Authorize(Roles = "Admin,TrafficOperator")]
    public async Task<ActionResult<NotificationResponse>> SendPublicNotice([FromBody] PublicNoticeRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Message) ||
            string.IsNullOrWhiteSpace(request.TargetAudience))
        {
            return BadRequest("Title, Message, and TargetAudience are required.");
        }

        await _notificationService.SendPublicNoticeAsync(request.Title, request.Message, request.TargetAudience);

        return Ok(new NotificationResponse
        {
            NotificationId = Guid.NewGuid(), 
            Type = "PublicNotice",
            Title = request.Title,
            Message = request.Message,
            RecipientEmail = request.TargetAudience, 
            Status = "Published",
            CreatedAt = DateTime.UtcNow
        });
    }

    // GET: /api/notifications/recipient/{email}
    [HttpGet("recipient/{email}")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetByRecipientEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        var notifications = await _notificationService.GetNotificationsByRecipientEmailAsync(email);
        return Ok(notifications.Select(n => new NotificationResponse
        {
            NotificationId = n.NotificationId,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            RecipientEmail = n.RecipientEmail,
            Status = n.Status,
            CreatedAt = n.CreatedAt,
            IsRead = n.IsRead 
        }));

    }

    // GET: /api/notifications/history/{userId}
    [HttpGet("history/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DeliveryLogResponse>>> GetUserHistory(Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest("UserId is required.");

        var logs = await _notificationService.GetDeliveryHistoryAsync(userId);
        return Ok(logs.Select(l => new DeliveryLogResponse
        {
            DeliveryId = l.DeliveryId,
            NotificationId = l.NotificationId,
            RecipientEmail = l.RecipientEmail,
            Status = l.Status,
            SentAt = l.SentAt,
            IsRead = l.IsRead
        }));
    }

    // PATCH: /api/notifications/{notificationId}/read
    [HttpPatch("{notificationId:guid}/read")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
    public async Task<ActionResult> MarkAsRead(Guid notificationId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized("Email not found in token");

        await _notificationService.MarkAsReadAsync(notificationId, email);
        return Ok(new { status = "read", notificationId });
    }

    // PATCH: /api/notifications/read-all
    [HttpPatch("read-all")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized("Email not found in token");

        await _notificationService.MarkAllAsReadAsync(email);
        return Ok(new { status = "all-read" });
    }
}
