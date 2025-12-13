using gameHubBack.DTOs;
using gameHubBack.entities;

namespace gameHubBack.extensions;

public static class AppUserExtensions
{
    public static UserDto ToDto(this AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            Password = user.Password,
        };
    }
}