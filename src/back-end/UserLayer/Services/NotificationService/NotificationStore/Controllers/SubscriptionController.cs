using Microsoft.AspNetCore.Mvc;
using NotificationStore.Business.Subscription;
using NotificationStore.Models.Requests;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Controllers;

[ApiController]
[Route("api/notifications/subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly INotificationSubscriptionService _subscriptionService;

    public SubscriptionController(
        INotificationSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost]
    [Route("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.UserEmail))
            return BadRequest("UserId and UserEmail are required.");

        var result = await _subscriptionService.SubscribeAsync(request);

        return Ok(result);
    }

    [HttpGet]
    [Route("{userId}")]
    public async Task<IActionResult> GetSubscriptions(string userId)
    {
        var result = await _subscriptionService.GetUserSubscriptionsAsync(userId);
        return Ok(result);
    }

    [HttpDelete]
    [Route("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] UnsubscribeRequest request)
    {
        if (string.IsNullOrEmpty(request.UserEmail))
            return BadRequest("UserEmail is required.");

        await _subscriptionService.UnsubscribeAsync(request);

        return NoContent();
    }
}
