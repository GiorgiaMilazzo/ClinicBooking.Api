using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;

namespace ClinicBooking.Api.Controllers
{
    [ApiController] //dice questa è un API, gestisci automaticamente la validazione dei dati in ingresso e le risposte in formato JSON
    [Route("api/[controller]")] //definisce la route/URL del controller
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // costruttore
        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<List<Appointment>> GetAppointmentAsync()
        {
            var query = await _context.Appointments.ToListAsync();
            return query;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetById(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            return appointment == null ? NotFound() : Ok(appointment);
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> AddAppointmentAsync(Appointment newAppointment)
        {
        // qui newAppointment.DoctorId e newAppointment.PatientId arrivano dal chiamante
        // come due semplici numeri interi (es. 5 e 12)

        var doctorExist = await _context.Doctors.FindAsync(newAppointment.DoctorId);
    
        // controlla che esista davvero un Doctor con quell'Id
        if(doctorExist == null)
            {
                return BadRequest("Il medico indicato non esiste.");
            }
        var patientExist = await _context.Patients.FindAsync(newAppointment.PatientId);

        // controlla che esista davvero un Patient con quell'Id
        if(patientExist == null)
            {
                return BadRequest("Il paziente indicato non esiste.");
            }   
        // controlla che quel Doctor non abbia già un altro Appointment
        // nello stesso identico DateTimeAppointment
        bool doctorBusy = await _context.Appointments.AnyAsync(a => 
        a.DoctorId == newAppointment.DoctorId &&
        a.DateTimeAppointment == newAppointment.DateTimeAppointment);

        if(doctorBusy)
            {
                return BadRequest("Il medico ha già un appuntamento in questo orario.");
            }
    
        await _context.Appointments.AddAsync(newAppointment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newAppointment.Id }, newAppointment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, Appointment updatedAppointment)
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);

            if(existingAppointment == null)
            {
                return NotFound();
            }
            existingAppointment.DateTimeAppointment = updatedAppointment.DateTimeAppointment;
            existingAppointment.Status = updatedAppointment.Status;
            await _context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var existingAppointment = await _context.Appointments.FindAsync();

            if(existingAppointment == null)
            {
                return NotFound();
            }
            _context.Remove(existingAppointment);
            await _context.SaveChangesAsync();

            return NoContent();
            
        }
    }
}