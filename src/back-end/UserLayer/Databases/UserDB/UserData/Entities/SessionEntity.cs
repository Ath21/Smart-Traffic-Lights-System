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

