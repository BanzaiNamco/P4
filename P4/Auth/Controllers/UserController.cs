using Auth.Data;
using Auth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[Route("")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public UserController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [Authorize]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User data)
    {
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
        var user = new User
        {
            IDno = data.IDno,
            Password = BCrypt.Net.BCrypt.HashPassword(data.Password),
            Type = data.Type
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Register), new { id = user.IDno }, user);
    }

}

