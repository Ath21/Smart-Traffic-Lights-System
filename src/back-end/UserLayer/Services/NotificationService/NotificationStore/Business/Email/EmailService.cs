using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationStore.Models;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Business.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly ILogPublisher _logPublisher;

    private const string ServiceTag = "[BUSINESS][EMAIL]";

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger,
        ILogPublisher logPublisher)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _logPublisher = logPublisher;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            _logger.LogWarning("{Tag} No recipient email provided, skipping email send.", ServiceTag);
            await _logPublisher.PublishAuditAsync(
                category: "Email",
                message: $"{ServiceTag} Skipped email send (missing recipient).");
            return;
        }

        try
        {
            // Build email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            message.Body = builder.ToMessageBody();

            // Send via SMTP
            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("{Tag} Email sent successfully to {Recipient}, Subject: {Subject}",
                ServiceTag, recipientEmail, subject);

            await _logPublisher.PublishAuditAsync(
                category: "Email",
                message: $"{ServiceTag} Email sent successfully",
                data: new()
                {
                    { "recipient", recipientEmail },
                    { "subject", subject }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to send email to {Recipient}, Subject: {Subject}",
                ServiceTag, recipientEmail, subject);

            await _logPublisher.PublishErrorAsync(
                category: "Email",
                message: $"{ServiceTag} Failed to send email to {recipientEmail}: {ex.Message}",
                data: new()
                {
                    { "recipient", recipientEmail },
                    { "subject", subject },
                    { "exception", ex.Message },
                    { "stack_trace", ex.StackTrace ?? string.Empty }
                });

            // Rethrow to let ExceptionMiddleware handle the HTTP response
            throw;
        }
    }
}
