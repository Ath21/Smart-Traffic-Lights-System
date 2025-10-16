using Messages.Traffic;
using Messages.User;

namespace NotificationStore.Business.MessageHandler;

public interface INotificationProcessor
{
    Task HandleUserNotificationAsync(UserNotificationMessage msg);
    Task HandleTrafficAnalyticsAsync(TrafficAnalyticsMessage msg);
}