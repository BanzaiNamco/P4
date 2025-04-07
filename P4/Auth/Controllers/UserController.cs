using Auth.Data;
using Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;

        public UserController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> register([FromBody] User data)
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
            data.Password = BCrypt.Net.BCrypt.HashPassword(data.Password);
            _dbContext.Users.Add(data);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "User registered successfully." });
        }

        [HttpGet]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> getAllProf()
        {
            var profs = await _dbContext.Users.Where(u => u.Type == "prof").Select(u => u.IDno).ToListAsync();
            if (profs == null || profs.Count == 0)
            {
                return NotFound("No professors found.");
            }
            return Ok(profs);
        }
    }
}
