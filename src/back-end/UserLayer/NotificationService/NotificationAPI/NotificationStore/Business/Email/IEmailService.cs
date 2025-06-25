/*
 * NotificationStore.Business.Email.IEmailService
 *
 * This file is part of the NotificationStore project, which defines the interface for email services.
 * The IEmailService interface declares a method for sending emails asynchronously.
 * It is used to abstract the email sending functionality, allowing for different implementations
 * (e.g., SMTP, third-party email services) without changing the service layer.
 * The interface is typically implemented by a class that handles the actual email sending logic.
 */
namespace NotificationStore.Business.Email;

public interface IEmailService
{
    Task SendEmailAsync(string recipientEmail, string subject, string body);
}
