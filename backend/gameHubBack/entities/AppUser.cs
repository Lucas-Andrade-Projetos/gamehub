namespace gameHubBack.Entities;

public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Email { get; set; }
    public required string Nickname { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string NormalizedNickname { get; set; }
    public string? ImageUrl { get; set; }
    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
}