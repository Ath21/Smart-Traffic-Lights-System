/*
 * UserData.Entities.User
 * 
 * This class represents a user entity in the system.
 * It contains properties for user details, including:
 * - UserId: Unique identifier for the user
 * - Username: The user's name
 * - PasswordHash: The hashed password for authentication
 * - Email: The user's email address
 * - Role: The role of the user (e.g., Admin, User)
 * - Status: The current status of the user (e.g., Active, Inactive)
 * - CreatedAt: Timestamp of when the user was created
 * - UpdatedAt: Timestamp of when the user was last updated
 *
 */
using System.ComponentModel.DataAnnotations;

namespace UserData.Entities;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }
    [Required]
    public string Role { get; set; } = "User";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Session> Sessions { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; }
}
