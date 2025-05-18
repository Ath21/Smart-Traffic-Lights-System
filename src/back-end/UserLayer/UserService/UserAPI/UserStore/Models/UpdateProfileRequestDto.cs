/*
 * UserStore.Models.UpdateProfileRequestDto
 *
 * This class represents the data transfer object (DTO) for update profile requests in the UserStore application.
 * It contains properties for the username, email, password, and confirm password fields, which are required for updating a user's profile.
 * The UpdateProfileRequestDto class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using System.ComponentModel.DataAnnotations;

namespace UserStore.Models;

public class UpdateProfileRequestDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}
