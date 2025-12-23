using System;
using gameHubBack.entities;

namespace gameHubBack.interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
}
