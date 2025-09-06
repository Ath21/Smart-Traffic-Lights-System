namespace UserStore.Models.Dtos;

public class SessionDto
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
