namespace NotificationStore.Business;

public interface INotificationProcessor
{
    Task ProcessNotificationAsync(UserNotificationMessage message);
}