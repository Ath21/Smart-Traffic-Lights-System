using System;
using System.ComponentModel.DataAnnotations;

namespace UserStore.Models;

public class RestPasswordDto
{
    [Required]
    public string UsernameOrEmail { get; set; }
    [Required]
    public string NewPassword { get; set; }
    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}
