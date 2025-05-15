using System;

namespace UserData.Entities;

public class Session
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
