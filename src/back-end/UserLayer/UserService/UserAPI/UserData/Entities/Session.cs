using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserData.Entities;

public class Session
{
    [Key]
    public Guid SessionId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public DateTime ExpiresAt { get; set; }
}
