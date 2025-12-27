using System.ComponentModel.DataAnnotations;

namespace gameHubBack.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(3)]
    public required string Nickname { get; set; }

    [Required]
    [MinLength(7)]
    public required string Password { get; set; }

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords doesn't match")]
    public required string ConfirmPassword { get; set; }
}
