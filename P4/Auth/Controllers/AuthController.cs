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

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
    {
        var result = await _jwtService.Authenticate(request.IDno, request.Password);
        if (result == null)
        {
            return Unauthorized();
        }
        return Ok(result);
    }



}

