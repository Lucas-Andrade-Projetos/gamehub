namespace gameHubBack.entities;

public class AppUser
{
    public string Id = Guid.NewGuid().ToString();
    public required string Email { get; set; }
    public required string Nickname { get; set; }
    public required string Password { get; set; }
    public byte[] PasswordSalt { get; set; } = [];
}