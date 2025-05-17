using System;
using System.ComponentModel.DataAnnotations;

namespace UserStore.Models;

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}
