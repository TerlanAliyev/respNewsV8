using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using respNewsV8.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RespNewContext _sql;
        private readonly IConfiguration _configuration; // IConfiguration 

        public UserController(RespNewContext sql, IConfiguration configuration)
        {
            _sql = sql;
            _configuration = configuration; // Configuration'ı al
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            if (IsValidUser(user))
            {
                var tokenString = GenerateJwtToken(user.UserName);
                return Ok(new { Token = tokenString });
            }

            return Unauthorized();
        }

        private string GenerateJwtToken(string username)
        {
            var user = _sql.Users.SingleOrDefault(x => x.UserName == username);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role,user.UserRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool IsValidUser(User user)
        {
            var foundUser = _sql.Users.SingleOrDefault(u => u.UserName == user.UserName && u.UserPassword == user.UserPassword);
            return foundUser != null;
        }





    }
}

