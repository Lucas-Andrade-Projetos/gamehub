using gameHubBack.Entities;

namespace gameHubBack.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
}
