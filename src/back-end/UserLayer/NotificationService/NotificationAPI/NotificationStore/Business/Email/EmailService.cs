/*
 * NotificationStore.Business.Email.EmailService
 *
 * This file is part of the NotificationStore project, which implements the EmailService class.
 * The EmailService class provides functionality to send emails using SMTP.
 * It implements the IEmailService interface and uses MailKit for email operations.
 * The class is responsible for constructing email messages and sending them to specified recipients.
 * It retrieves email settings from the configuration and uses them to connect to the SMTP server.
 * The EmailService class is typically used in the NotificationService layer of the application.
 * It is part of the NotificationStore project, which is responsible for managing notifications
 * and related operations in the system.
 */
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationStore.Models;

namespace NotificationStore.Business.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        message.To.Add(new MailboxAddress("", recipientEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = builder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
