using Auth.Model;
using Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[Route("")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;
    public AuthController(JwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel data)
    {
        var result = await _jwtService.Authenticate(data.IDno, data.Password);
        if (result == null)
        {
            return Unauthorized();
        }
        return Ok(result);
    }

    [HttpGet("getinfo")]
    public IActionResult GetUserInfo()
    {
        // Log the token
        var token = HttpContext.Request.Headers["Authorization"].ToString();
        _logger.LogInformation("User Identity: {UserIdentity}", User.Identity?.Name);
        _logger.LogInformation("User Claims:");
        foreach (var claim in User.Claims)
        {
            _logger.LogInformation("{ClaimType}: {ClaimValue}", claim.Type, claim.Value);
        }


        return Ok(token);
    }

}

