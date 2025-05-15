using System;

namespace UserStore.Models;

public class AuthResponseDto
{
    public bool IsAuthenticated { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
