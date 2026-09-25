using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;
using ClinicBooking.Api.Dtos;
using ClinicBooking.Api.Services;

namespace ClinicBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;   // per leggere appsettings.json

        public LoginController(AppDbContext context, PasswordService passwordService, IConfiguration configuration)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult> LoginAsync(LoginDto loginDto)
        {
            // 1. Trova l'utente per Email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return Unauthorized("Email o password errati.");
            }

            // 2. Verifica la password (usa il metodo che già conosci)
            bool passwordOk = _passwordService.VerifyPassword(user, user.PasswordHash, loginDto.Password);
            if (!passwordOk)
            {
                return Unauthorized("Email o password errati.");
            }

            // 3. Costruisci le Claims (dati che finiranno nel payload del token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // 4. Prepara la chiave di firma, letta da appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Costruisci il token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            // 6. Restituisci il token come stringa
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}