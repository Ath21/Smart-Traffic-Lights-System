/*
 * NotificationStore.Models.EmailSettings
 *
 * This file is part of the NotificationStore project, which defines the EmailSettings model.
 * The EmailSettings class contains properties for configuring email notifications,
 * including SMTP server details, sender information, and authentication credentials.
 * It is used to manage email settings for sending notifications in the system.
 */
namespace NotificationStore.Models;

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int Port { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
