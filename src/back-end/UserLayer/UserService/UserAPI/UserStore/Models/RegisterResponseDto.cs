/*
 * UserStore.Models.RegisterResponseDto
 *
 * This class represents the data transfer object (DTO) for registration responses in the UserStore application.
 * It contains properties for the user ID, username, email, role, and status of the registered user.
 * The RegisterResponseDto class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
namespace UserStore.Models;

public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Status { get; set; }
}
