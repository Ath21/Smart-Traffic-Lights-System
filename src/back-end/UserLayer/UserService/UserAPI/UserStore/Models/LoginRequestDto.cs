/*
 * UserStore.Models.LoginRequestDto
 *
 * This class represents the data transfer object (DTO) for login requests in the UserStore application.
 * It contains properties for the username and password fields, which are required for user authentication.
 * The LoginRequestDto class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using System.ComponentModel.DataAnnotations;

namespace UserStore.Models;

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}
