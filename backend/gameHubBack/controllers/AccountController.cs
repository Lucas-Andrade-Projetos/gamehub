using System.Security.Cryptography;
using System.Text;
using gameHubBack.DTOs;
using gameHubBack.entities;
using gameHubBack.extensions;
using gameHubBack.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace gameHubBack.controllers;

[ApiController]
[Route("api/{controller}")]
public class AccountController(Users users, ITokenService tokenService) : ControllerBase
{
    readonly private Users _users = users;

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (_users.UserList.Any(x => x.Email.ToLower() == registerDto.Email.ToLower()))
            return Unauthorized("Email is already taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Email = registerDto.Email,
            Nickname = registerDto.Nickname,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _users.UserList.Add(user);

        return user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = _users.UserList.SingleOrDefault(u => u.Email == loginDto.Email);

        if (user == null) return Unauthorized("Invalid email or password");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (var i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid email or password");
        }

        return user.ToDto(tokenService);
    }
}