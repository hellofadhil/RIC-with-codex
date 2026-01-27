using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace OnePro.API.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly OneProDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(OneProDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ============================================================
        // REGISTER
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (await EmailExists(model.Email))
                return BadRequest("Email already exists");

            var user = CreateUser(model);

            await _db.Users!.AddAsync(user);
            await _db.SaveChangesAsync();

            return Ok(
                new
                {
                    message = "Register success",
                    user = SerializeUser(user),
                    token = GenerateJwtToken(user),
                }
            );
        }

        // ============================================================
        // LOGIN
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null || !VerifyPassword(user.PasswordHash, model.Password))
                return Unauthorized("Invalid email/password");

            return Ok(
                new
                {
                    message = "Login success",
                    user = SerializeUser(user),
                    token = GenerateJwtToken(user),
                }
            );
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        private static object SerializeUser(User user) =>
            new
            {
                user.Id,
                user.Email,
                user.Name,
                user.Role,
                user.IdGroup,
            };

        private static User CreateUser(RegisterRequest model) =>
            new()
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                Name = model.Name,
                Position = model.Position,
                IdGroup = model.IdGroup,
                Role = Role.User_Member,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            };

        private Task<bool> EmailExists(string email) => _db.Users!.AnyAsync(x => x.Email == email);

        private static bool VerifyPassword(string hash, string password) =>
            BCrypt.Net.BCrypt.Verify(password, hash);

        // ============================================================
        // JWT Generator
        // ============================================================
        private string GenerateJwtToken(User user)
        {
            string key = _config["Key:Jwt"] ?? throw new Exception("Missing Key:Jwt");

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("name", user.Name),
                new Claim("role", user.Role.ToString()),
                new Claim("groupId", user.IdGroup.ToString()),
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddMinutes(60),
                claims: claims,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
