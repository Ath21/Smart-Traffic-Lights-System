using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserData.Entities;

// Updated by : User Service
// Read by    : User Service
[Table("users")]
public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string? Username { get; set; }

    [Required, MaxLength(100)]
    public string? Email { get; set; }

    [Required]
    public string? PasswordHash { get; set; }

    [MaxLength(20)]
    public string? Role { get; set; } // = "User", "Admin", "TrafficOperator";

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<SessionEntity>? Sessions { get; set; } 
    public ICollection<UserAuditEntity>? Audits { get; set; }
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