using gameHubBack.DTOs;
using gameHubBack.entities;
using gameHubBack.interfaces;

namespace gameHubBack.extensions;

public static class AppUserExtensions
{
    public static UserDto ToDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDto
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            Token = tokenService.CreateToken(user)
        };
    }
}