using gameHubBack.DTOs;
using gameHubBack.Entities;
using gameHubBack.Interfaces;

namespace gameHubBack.Extensions;

public static class AppUserExtensions
{
    public static UserDto ToDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDto
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            ImageUrl = user.ImageUrl,
            Token = tokenService.CreateToken(user)
        };
    }
}