using System;

namespace UserStore.Models;

public class LoginResponseDto
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
