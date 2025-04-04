using Auth.Data;
using Auth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Controllers;

[Route("")]
[ApiController]
public class UserAccountController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    public UserAccountController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<List<User>> Get()
    {
        return await _dbContext.Users.ToListAsync();
    }

    [Authorize]
    [HttpGet("id")]
    public async Task<User?> GetById(string id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.IDno == id);
    }
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.IDno) ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            string.IsNullOrWhiteSpace(user.Type))
        {
            return BadRequest("Invalid user data.");
        }
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.IDno }, user);
    }

    [HttpGet]
    public IActionResult Test()
    {
        // Retrieve the token from the Authorization header
        string token = Request.Headers["Authorization"];

        // If the token is missing or doesn't start with "Bearer", return unauthorized
        if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer"))
        {
            return Unauthorized("Authorization header is missing or not formatted correctly.");
        }

        // Extract the actual token (removing the "Bearer " prefix)
        token = token.Substring("Bearer ".Length).Trim();

        // Now we can safely handle the JWT token
        var handler = new JwtSecurityTokenHandler();

        try
        {
            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            var claims = new Dictionary<string, string>();

            // Iterate through claims and add them to the dictionary
            foreach (var claim in jwt.Claims)
            {
                claims.Add(claim.Type, claim.Value);
            }

            return Ok(claims);  // Return the claims from the JWT
        }
        catch (Exception ex)
        {
            return Unauthorized($"Invalid token: {ex.Message}");  // Handle any exceptions
        }
    }

}
