using Auth.Models;
using Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [Route("[action]")]
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
        public async Task<IActionResult> login([FromBody] LoginRequest data)
        {
            if (String.IsNullOrEmpty(data.IDno))
            {
                return BadRequest(new { message = "ID number needs to entered" });
            }
            else if (String.IsNullOrEmpty(data.Password))
            {
                return BadRequest(new { message = "Password needs to entered" });
            }

            string result = await _jwtService.login(data.IDno, data.Password);

            if (result == null)
            {
                return Unauthorized();
            }

            return Ok(result);
        }

    }
}
