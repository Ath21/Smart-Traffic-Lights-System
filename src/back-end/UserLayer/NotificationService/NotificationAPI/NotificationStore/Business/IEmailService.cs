using System;

namespace NotificationStore.Business;

public interface IEmailService
{
    Task SendEmailAsync(string recipientEmail, string subject, string body);
}
