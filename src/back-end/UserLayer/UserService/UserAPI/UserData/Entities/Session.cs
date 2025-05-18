/*
 * UserData.Entities.Session
 *
 * This class represents a session entity in the system.
 * It contains properties for session details, including:
 * - SessionId: Unique identifier for the session
 * - UserId: Identifier for the user associated with the session
 * - User: Navigation property to the User entity
 * - Token: The session token for authentication
 * - ExpiresAt: Timestamp of when the session expires
 *
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserData.Entities;

public class Session
{
    [Key]
    public Guid SessionId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public DateTime ExpiresAt { get; set; }
}
