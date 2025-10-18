using System;
using Messages.User;
using UserStore.Models.Requests;
using UserStore.Models.Responses;
using UserStore.Publishers.Logs;
using UserStore.Publishers.Notifications;

namespace UserStore.Business.Subscribe;

public class UserSubscriptionService : IUserSubscriptionService
{
    private readonly IUserNotificationPublisher _notificationPublisher;
    private readonly IUserLogPublisher _logPublisher;
    private readonly ILogger<UserSubscriptionService> _logger;

    private const string ServiceTag = "[BUSINESS][SUBSCRIPTION]";

    public UserSubscriptionService(
        IUserNotificationPublisher notificationPublisher,
        IUserLogPublisher logPublisher,
        ILogger<UserSubscriptionService> logger)
    {
        _notificationPublisher = notificationPublisher;
        _logPublisher = logPublisher;
        _logger = logger;
    }

    public async Task<SubscriptionResponse> SubscribeAsync(string userId, string email, UserSubscriptionRequest request)
    {
        var message = new UserNotificationRequest
        {
            UserId = userId,
            UserEmail = email,
            Intersection = request.Intersection,
            Metric = request.Metric
        };

        try
        {
            await _notificationPublisher.PublishSubscriptionRequestAsync(message);

            _logger.LogInformation("{Tag} Published subscription request for {UserId} ({Email}) → {Intersection}/{Metric}",
                ServiceTag, userId, email, request.Intersection, request.Metric);

            await _logPublisher.PublishAuditAsync(
                source: "user-api",
                messageText: $"{ServiceTag} Subscription request published successfully.",
                category: "SUBSCRIBE",
                data: new Dictionary<string, object>
                {
                    ["UserId"] = userId,
                    ["Email"] = email,
                    ["Intersection"] = request.Intersection,
                    ["Metric"] = request.Metric
                });

            return new SubscriptionResponse
            {
                UserId = userId,
                UserEmail = email,
                Intersection = request.Intersection,
                Metric = request.Metric,
                Status = "Published"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish subscription request for {UserId} ({Email})", ServiceTag, userId, email);

            await _logPublisher.PublishErrorAsync(
                source: "user-api",
                messageText: $"{ServiceTag} Failed to publish subscription request: {ex.Message}",
                data: new Dictionary<string, object>
                {
                    ["UserId"] = userId,
                    ["Email"] = email,
                    ["Intersection"] = request.Intersection,
                    ["Metric"] = request.Metric,
                    ["Exception"] = ex.Message
                });

            throw;
        }
    }
}
