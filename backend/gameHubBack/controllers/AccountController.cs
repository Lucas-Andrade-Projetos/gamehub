using System.Security.Cryptography;
using System.Text;
using gameHubBack.Data;
using gameHubBack.DTOs;
using gameHubBack.Entities;
using gameHubBack.Extensions;
using gameHubBack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace gameHubBack.Controllers;

[ApiController]
[Route("api/{controller}")]
public class AccountController(AppDbContext context, ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (context.Users.Any(x => x.Email.ToLower() == registerDto.Email.ToLower()))
            return Unauthorized("Email is already taken");

        if (context.Users.Any(x => x.Nickname.ToLower() == registerDto.Nickname.ToLower()))
            return Unauthorized("Nickname is already taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Email = registerDto.Email,
            Nickname = registerDto.Nickname,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = context.Users.FirstOrDefault(u => u.Email.ToLower() == loginDto.Email.ToLower());

        if (user == null) return Unauthorized("Invalid email or password");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            return Unauthorized("Invalid email or password");

        return user.ToDto(tokenService);
    }
}