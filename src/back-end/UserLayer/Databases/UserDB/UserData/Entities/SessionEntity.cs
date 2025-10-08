using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserData.Entities;

// Updated by : User Service
// Read by    : User Service
[Table("sessions")]
public class SessionEntity
{
    // unique session ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SessionId { get; set; }

     // foreign key to user
    [Required]
    public int UserId { get; set; }

    // navigation property to user
    [ForeignKey(nameof(UserId))]
    public UserEntity? User { get; set; }

    // JWT token
    public string Session { get; set; } = string.Empty;

    // login timestamp (UTC)
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;

    // logout timestamp (nullable)
    public DateTime? LogoutTime { get; set; }

    // session active flag
    public bool IsActive { get; set; } = true;
}

/*

{
  "SessionId": 101,
  "UserId": 1,
  "Session": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "LoginTime": "2025-10-08T08:10:00Z",
  "LogoutTime": null,
  "IsActive": true
},
{
  "SessionId": 102,
  "UserId": 2,
  "Session": "eyJhbGc...trafficop_token",
  "LoginTime": "2025-10-08T08:30:00Z",
  "LogoutTime": null,
  "IsActive": true
}

*/