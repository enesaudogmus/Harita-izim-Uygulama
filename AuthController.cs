using Microsoft.AspNetCore.Mvc;
using DrawingApp.Models;
using DrawingApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DrawingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest("Kullanıcı zaten mevcut.");

            user.PasswordHash = HashPassword(user.PasswordHash); 
            user.Role = "User"; 

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Kullanıcı oluşturuldu.");
        }

[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] User user)
{
    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
    if (existingUser == null || !VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
    {
        return Unauthorized("Kullanıcı adı veya şifre yanlış.");
    }

    var token = GenerateJwtToken(existingUser);
    return Ok(new { token });
}


        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
#pragma warning disable CS8604 
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
#pragma warning restore CS8604 

            var tokenHandler = new JwtSecurityTokenHandler();
#pragma warning disable CS8604 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = System.DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
#pragma warning restore CS8604 

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string inputPassword, string storedHash)
        {
            return HashPassword(inputPassword) == storedHash;
        }
    }
}
