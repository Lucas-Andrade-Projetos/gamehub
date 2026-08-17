using System.ComponentModel.DataAnnotations;

namespace gameHubBack.DTOs;

public class UpdateProfileDto
{
    [Required]
    [MinLength(3)]
    public required string Nickname { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    // null = remove the current photo (fall back to the default bust icon on the client).
    // empty/omitted = leave the photo untouched. Any other value = the new base64 data URL.
    [MaxLength(700_000)]
    public string? ImageBase64 { get; set; }

    public bool ImageChanged { get; set; }
}
