namespace gameHubBack.entities;

public class AppUser
{
    public string Id = Guid.NewGuid().ToString();
    public required string Email { get; set; }
    public required string Nickname { get; set; }
    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
}