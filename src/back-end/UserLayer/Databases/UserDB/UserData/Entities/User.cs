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
    public UserRole Role { get; set; } = UserRole.User;

    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Session> Sessions { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; }
}
