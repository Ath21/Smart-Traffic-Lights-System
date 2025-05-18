/*
 * UserStore.Models.ResetPasswordRequestDto
 *
 * This class represents the data transfer object (DTO) for reset password requests in the UserStore application.
 * It contains properties for the username or email, new password, and confirm password fields, which are required for resetting a user's password.
 * The ResetPasswordRequestDto class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using System.ComponentModel.DataAnnotations;

namespace UserStore.Models;

public class ResetPasswordRequestDto
{
    [Required]
    public string UsernameOrEmail { get; set; }
    [Required]
    public string NewPassword { get; set; }
    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}
