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
    public string Role { get; set; } = UserRole.TrafficOperator;

    // active/inactive account flag
    public bool IsActive { get; set; } = true;

    // account creation timestamp (UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // user sessions
    public ICollection<SessionEntity> Sessions { get; set; } = new List<SessionEntity>();

    // audit log entries
    public ICollection<UserAuditEntity> Audits { get; set; } = new List<UserAuditEntity>();
}

