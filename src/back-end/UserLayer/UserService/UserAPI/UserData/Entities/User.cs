using System;
using System.ComponentModel.DataAnnotations;

namespace UserData.Entities;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and numbers.")]
    public string Username { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
    [Required]
    public string Role { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Session> Sessions { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; }
}
