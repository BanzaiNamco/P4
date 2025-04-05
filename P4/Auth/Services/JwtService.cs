using Auth.Data;
using Auth.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Services
{
    using BCrypt.Net;

    public class JwtService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public JwtService(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<string> Authenticate(string IDNo, string password)
        {
            User? user = await _dbContext.Users.FindAsync(IDNo);

            if (user == null || BCrypt.Verify(password, user.Password) == false)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.IDno),
                    new Claim(ClaimTypes.Role, user.Type)
                }),
                IssuedAt = DateTime.UtcNow,
                Issuer = _configuration["JwtConfig:Issuer"],
                Audience = _configuration["JwtConfig:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            tokenHandler.WriteToken(token);
            string jwt = tokenHandler.WriteToken(token);

            return jwt;
        }
    }
}