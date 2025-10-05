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
