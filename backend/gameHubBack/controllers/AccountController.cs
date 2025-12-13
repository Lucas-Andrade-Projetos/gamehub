using gameHubBack.DTOs;
using gameHubBack.entities;
using gameHubBack.extensions;
using Microsoft.AspNetCore.Mvc;

namespace gameHubBack.controllers;

[ApiController]
[Route("api/{controller}")]
public class AccountController : ControllerBase
{
    readonly private Users _users;

    public AccountController(Users users)
    {
        _users = users;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            Email = registerDto.Email,
            Nickname = registerDto.Nickname,
            Password = registerDto.Password
        };

        _users.UserList.Add(user);

        return user.ToDto();
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {

        foreach (var user in _users.UserList)
        {
            if (user.Email == loginDto.Email && user.Password == loginDto.Password)
            {
                return Ok(user.ToDto());
            }
        }

        return Unauthorized("Invalid email or password");
    }
}
