using Microsoft.AspNetCore.Mvc;
using NotificationStore.Business.Delivery;
using NotificationStore.Models.Requests;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Controllers;

[ApiController]
[Route("api/notifications/deliveries")]
public class DeliveryController : ControllerBase
{
    private readonly INotificationDeliveryService _deliveryService;
    private readonly ILogPublisher _logPublisher;

    public DeliveryController(
        INotificationDeliveryService deliveryService,
        ILogPublisher logPublisher)
    {
        _deliveryService = deliveryService;
        _logPublisher = logPublisher;
    }

    [HttpGet]
    [Route("{userId}")]
    public async Task<IActionResult> GetDeliveries(string userId, [FromQuery] bool unreadOnly = false)
    {
        var logs = await _deliveryService.GetUserDeliveriesAsync(userId, unreadOnly);

        await _logPublisher.PublishAuditAsync(
            "Controller",
            $"[API][DELIVERY] GET deliveries for {userId} (UnreadOnly={unreadOnly})");

        return Ok(logs);
    }

    [HttpPatch]
    [Route("read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.UserEmail))
            return BadRequest("UserId and UserEmail are required.");

        await _deliveryService.MarkAsReadAsync(request);

        await _logPublisher.PublishAuditAsync(
            "Controller",
            $"[API][DELIVERY] Marked {request.DeliveryId} as read for {request.UserId} ({request.UserEmail})");

        return Ok(new { status = "read", request.DeliveryId });
    }
}
