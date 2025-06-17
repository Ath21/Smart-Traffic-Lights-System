/*
 * LogStore.Models.LogDto
 *
 * This class represents a Data Transfer Object (DTO) for logging information.
 * It contains properties for the log level, service name, message, and timestamp.
 * The LogDto is used to encapsulate log data that can be sent over the network or
 * stored in a database.
 * It is typically used in logging services to standardize the format of log entries.
 * The properties are initialized with default values to ensure that they are not null.
 * The LogLevel defaults to "info", Service to an empty string, Message to an empty string,
 * and Timestamp to the current UTC time.
 */
namespace LogStore.Models;

public class LogDto
{
    public string LogLevel { get; set; } = "info";
    public string Service { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
