/*
 * UserData.Entities.AuditLog
 *
 * This class represents an audit log entity in the system.
 * It contains properties for audit log details, including:
 * - LogId: Unique identifier for the log entry
 * - UserId: Identifier for the user associated with the log entry
 * - User: Navigation property to the User entity
 * - Action: The action performed by the user
 * - Timestamp: Timestamp of when the action occurred
 * - Details: Additional details about the action
 *
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserData.Entities;

public class AuditLog
{
    [Key]
    public Guid LogId { get; set; }
    public Guid? UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    [Required]
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}
