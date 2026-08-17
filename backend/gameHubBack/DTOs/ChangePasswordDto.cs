using System.ComponentModel.DataAnnotations;

namespace gameHubBack.DTOs;

public class ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(12)]
    public required string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords doesn't match")]
    public required string ConfirmNewPassword { get; set; }
}
