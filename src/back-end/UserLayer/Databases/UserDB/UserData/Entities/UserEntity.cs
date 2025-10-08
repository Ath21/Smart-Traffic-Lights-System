using System.ComponentModel.DataAnnotations;

namespace UserData.Entities;

// Updated by : User Service
// Read by    : User Service
[Table("users")]
public class UserEntity
{
    // unique user ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    // username (e.g. "vmamalis")
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    // user email (e.g. "vmamalis@uniwa.gr")
    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;   

    // password hash for authentication
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // user role (User, Admin, TrafficOperator, Guest)
    [MaxLength(20)]
    public string Role { get; set; } = "User";

    // active/inactive account flag
    public bool IsActive { get; set; } = true;

    // account creation timestamp (UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // user sessions
    public ICollection<SessionEntity> Sessions { get; set; } = new List<SessionEntity>();

    // audit log entries
    public ICollection<UserAuditEntity> Audits { get; set; } = new List<UserAuditEntity>();
}

/*

{
  "UserId": 1,
  "Username": "vathanas1ou",
  "Email": "vathanas1ou@uniwa.gr",
  "PasswordHash": "$2a$10$...hashed_password...",
  "Role": "Admin",
  "IsActive": true,
  "CreatedAt": "2025-10-08T07:15:00Z"
},
{
  "UserId": 2,
  "Username": "vmamalis",
  "Email": "vmamalis@uniwa.gr",
  "PasswordHash": "$2a$10$...hashed_password...",
  "Role": "TrafficOperator",
  "IsActive": true,
  "CreatedAt": "2025-10-08T07:18:00Z"
},
{
  "UserId": 3,
  "Username": "apostolos",
  "Email": "apostolos@uniwa.gr",
  "PasswordHash": "$2a$10$...hashed_password...",
  "Role": "User",
  "IsActive": true,
  "CreatedAt": "2025-10-08T07:22:00Z"
}

*/