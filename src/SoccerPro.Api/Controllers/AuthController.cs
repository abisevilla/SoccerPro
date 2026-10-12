using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SoccerPro.Api.Authentication;

namespace SoccerPro.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(JwtTokenService tokenService, IOptions<JwtOptions> options) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult CreateToken(TokenRequest request)
    {
        var jwtOptions = options.Value;
        if (!string.Equals(request.Username, jwtOptions.Username, StringComparison.Ordinal) ||
            !string.Equals(request.Password, jwtOptions.Password, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        return Ok(new
        {
            accessToken = tokenService.CreateToken(request.Username),
            tokenType = "Bearer",
            expiresIn = TimeSpan.FromMinutes(jwtOptions.ExpirationMinutes).TotalSeconds
        });
    }
}

public sealed record TokenRequest(string Username, string Password);
