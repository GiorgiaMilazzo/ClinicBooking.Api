using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;

namespace ClinicBooking.Api.Controllers
{
    [ApiController] //dice questa è un API, gestisci automaticamente la validazione dei dati in ingresso e le risposte in formato JSON
    [Route("api/[controller]")] //definisce la route/URL del controller
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // costruttore
        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<List<Doctor>> GetDoctorsAsync()
        {
            var query = await _context.Doctors.ToListAsync();
            return query;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetById(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            return doctor == null ? NotFound() : Ok(doctor);
        }

        [HttpPost]
        public async Task<ActionResult<Doctor>> AddDoctorAsync(Doctor newDoctor)
        {
            await _context.Doctors.AddAsync(newDoctor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = newDoctor.Id}, newDoctor);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, Doctor updatedDoctor)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);

            if(existingDoctor == null)
            {
                return NotFound();
            }
            existingDoctor.Name = updatedDoctor.Name;
            existingDoctor.Lastname = updatedDoctor.Lastname;
            existingDoctor.Specialization = updatedDoctor.Specialization;
            await _context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var existingDoctor = await _context.Doctors.FindAsync();

            if(existingDoctor == null)
            {
                return NotFound();
            }
            _context.Remove(existingDoctor);
            await _context.SaveChangesAsync();

            return NoContent();
            
        }
    }
}