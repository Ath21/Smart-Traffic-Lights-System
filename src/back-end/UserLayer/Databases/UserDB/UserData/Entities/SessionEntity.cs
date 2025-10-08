using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserData.Entities;

// Updated by : User Service
// Read by    : User Service
[Table("sessions")]
public class SessionEntity
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int SessionId { get; set; }

  [Required]
  public int UserId { get; set; }

  [ForeignKey(nameof(UserId))]
  public UserEntity? User { get; set; }

  public string? Session { get; set; }
  public DateTime LoginTime { get; set; }
  public DateTime? LogoutTime { get; set; }
  public bool IsActive { get; set; }
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