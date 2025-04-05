using Auth.Data;
using Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext dbContext, ILogger<UserController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> register([FromBody] User data)
        {
            _logger.LogInformation("Registering user with ID: {id}", data?.IDno);
            _logger.LogInformation("User data: {password}", data.Password);
            _logger.LogInformation("User data: {type}", data.Type);
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            if (string.IsNullOrEmpty(data.IDno) || string.IsNullOrEmpty(data.Password) || string.IsNullOrEmpty(data.Type))
            {
                return BadRequest("IDno, Password, and Type are required.");
            }
            if (await _dbContext.Users.FindAsync(data.IDno) != null)
            {
                return Conflict("User already exists.");
            }
            if (data.Type != "prof" && data.Type != "student" && data.Type != "admin")
            {
                return BadRequest("Invalid user type.");
            }
            data.Password = BCrypt.Net.BCrypt.HashPassword(data.Password);
            _dbContext.Users.Add(data);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User registered successfully: {id}", data.IDno);
            return Ok(new { message = "User registered successfully." });
        }
    }
}
