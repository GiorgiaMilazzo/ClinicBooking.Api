using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;

namespace ClinicBooking.Api.Controllers
{
    [ApiController] //dice questa è un API, gestisci automaticamente la validazione dei dati in ingresso e le risposte in formato JSON
    [Route("api/[controller]")] //definisce la route/URL del controller
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // costruttore
        public PatientsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<List<Patient>> GetPatientsAsync()
        {
            var query = await _context.Patients.ToListAsync();
            return query;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetById(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            return patient == null ? NotFound() : Ok(patient);
        }

        [HttpPost]
        public async Task<ActionResult<Patient>> AddPatientAsync(Patient newPatient)
        {
            await _context.Patients.AddAsync(newPatient);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = newPatient.Id}, newPatient);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, Patient updatedPatient)
        {
            var existingPatient = await _context.Patients.FindAsync(id);

            if(existingPatient == null)
            {
                return NotFound();
            }
            existingPatient.Name = updatedPatient.Name;
            existingPatient.Lastname = updatedPatient.Lastname;
            existingPatient.DateOfBirth = updatedPatient.DateOfBirth;
            await _context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var existingPatient = await _context.Patients.FindAsync();

            if(existingPatient == null)
            {
                return NotFound();
            }
            _context.Remove(existingPatient);
            await _context.SaveChangesAsync();

            return NoContent();
            
        }
    }
}