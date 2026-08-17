using System.Security.Cryptography;
using System.Text;
using gameHubBack.Data;
using gameHubBack.DTOs;
using gameHubBack.Entities;
using gameHubBack.Extensions;
using gameHubBack.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace gameHubBack.Controllers;

[ApiController]
[Route("api/{controller}")]
public class AccountController(AppDbContext context, ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var normalizedEmail = registerDto.Email.ToUpperInvariant();
        var normalizedNickname = registerDto.Nickname.ToUpperInvariant();

        if (context.Users.Any(x => x.NormalizedEmail == normalizedEmail))
            return Unauthorized("Email is already taken");

        if (context.Users.Any(x => x.NormalizedNickname == normalizedNickname))
            return Unauthorized("Nickname is already taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Email = registerDto.Email,
            Nickname = registerDto.Nickname,
            NormalizedEmail = normalizedEmail,
            NormalizedNickname = normalizedNickname,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // the Any() checks above are a fast path for a friendly message; this catches the
            // race where two requests for the same email/nickname pass those checks concurrently -
            // the unique index on NormalizedEmail/NormalizedNickname is the real guarantee.
            return Unauthorized("Email or nickname is already taken");
        }

        return user.ToDto(tokenService);
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var normalizedEmail = loginDto.Email.ToUpperInvariant();
        var user = context.Users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail);

        if (user == null) return Unauthorized("Invalid email or password");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            return Unauthorized("Invalid email or password");

        return user.ToDto(tokenService);
    }
}