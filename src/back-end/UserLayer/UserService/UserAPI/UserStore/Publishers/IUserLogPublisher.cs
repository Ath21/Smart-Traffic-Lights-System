using System;

namespace UserStore.Publishers;

public interface IUserLogPublisher
{
    Task PublishInfoAsync(string message);
    Task PublishAuditAsync(Guid userId, string action, string details);
    Task PublishErrorAsync(string message, Exception exception);
}
