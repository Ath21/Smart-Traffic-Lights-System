using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserData.Entities;

// Updated by : User Service
// Read by    : User Service
[Table("user_audits")]
public class UserAuditEntity
{
    // unique audit ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AuditId { get; set; } 

    // foreign key to user
    [Required]
    public int UserId { get; set; }

    // navigation property to user
    [ForeignKey(nameof(UserId))]
    public UserEntity? User { get; set; }

    // action performed (e.g. "LOGIN_SUCCESS")
    [Required, MaxLength(200)]
    public string Action { get; set; } = string.Empty; 

    // additional details or JSON context
    [MaxLength(500)]
    public string Details { get; set; } = string.Empty;

    // timestamp of event (UTC)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/*

{
  "AuditId": 501,
  "UserId": 1,
  "Action": "CREATE_USER",
  "Details": "Admin 'vathanas1ou' created user 'vmamalis'.",
  "Timestamp": "2025-10-08T09:00:00Z"
},
{
  "AuditId": 502,
  "UserId": 2,
  "Action": "MANUAL_OVERRIDE",
  "Details": "Traffic operator 'vmamalis' set 'Agiou Spyridonos' intersection to 'Manual Mode'.",
  "Timestamp": "2025-10-08T09:20:00Z"
},
{
  "AuditId": 503,
  "UserId": 3,
  "Action": "USER_LOGIN",
  "Details": "User 'apostolos' logged in via web portal.",
  "Timestamp": "2025-10-08T09:25:00Z"
}

*/