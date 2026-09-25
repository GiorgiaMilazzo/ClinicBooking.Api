using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;
using ClinicBooking.Api.Dtos;
using ClinicBooking.Api.Services;

namespace ClinicBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public RegisterController(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPost]
        public async Task<ActionResult> RegisterAsync(RegisterDto registerDto)
        {
            // 1. Controlla che non esista già un utente con la stessa Email
            //    (usa AnyAsync, come hai già fatto per il controllo di sovrapposizione orari)
            bool emailTaken = await _context.Users.AnyAsync(u => u.Email == registerDto.Email);
            if (emailTaken)
            {
                return BadRequest("Email già registrata.");
            }

            // 2. Crea un nuovo User (senza PasswordHash per ora — non puoi calcolarlo
            //    prima di avere l'oggetto User stesso, il metodo HashPassword ne ha bisogno)
            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PasswordHash = "temp"   // valore temporaneo, lo sistemiamo subito sotto
            };

            // 3. Ora calcola l'hash vero, passando l'oggetto 'user' appena creato
            user.PasswordHash = _passwordService.HashPassword(user, registerDto.Password);

            // 4. Salva
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok("Registrazione completata.");
        }
    }
}