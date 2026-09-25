using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;
using ClinicBooking.Api.Dtos;

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
        public async Task<List<AppointmentDto>> GetAppointmentsAsync()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .ToListAsync();

            var appointmentDtos = new List<AppointmentDto>();

            foreach (var appointment in appointments)
            {
                appointmentDtos.Add(new AppointmentDto
                {
                    Id = appointment.Id,
                    DateTimeAppointment = appointment.DateTimeAppointment,
                    Status = appointment.Status,
                    DoctorName = appointment.Doctor.Name + " " + appointment.Doctor.Lastname,
                    PatientName = appointment.Patient.Name + " " + appointment.Patient.Lastname
                });
            }

            return appointmentDtos;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetById(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var appointmentDto = new AppointmentDto
            {
                Id = appointment.Id,
                DateTimeAppointment = appointment.DateTimeAppointment,
                Status = appointment.Status,
                DoctorName = appointment.Doctor.Name + " " + appointment.Doctor.Lastname,
                PatientName = appointment.Patient.Name + " " + appointment.Patient.Lastname
            };

            return Ok(appointmentDto);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> AddAppointmentAsync(AppointmentCreateDto newAppointmentDto)
        {
            var doctorExist = await _context.Doctors.FindAsync(newAppointmentDto.DoctorId);
            if (doctorExist == null)
            {
                return BadRequest("Il medico indicato non esiste.");
            }

            var patientExist = await _context.Patients.FindAsync(newAppointmentDto.PatientId);
            if (patientExist == null)
            {
                return BadRequest("Il paziente indicato non esiste.");
            }

            bool doctorBusy = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == newAppointmentDto.DoctorId &&
                a.DateTimeAppointment == newAppointmentDto.DateTimeAppointment);

            if (doctorBusy)
            {
                return BadRequest("Il medico ha già un appuntamento in questo orario.");
            }

            // costruisci l'Appointment vero, decidendo tu lo Status iniziale (ricordi la scelta fatta prima?)
            var appointment = new Appointment
            {
                DateTimeAppointment = newAppointmentDto.DateTimeAppointment,
                DoctorId = newAppointmentDto.DoctorId,
                PatientId = newAppointmentDto.PatientId,
                Status = AppointmentStatus.booked // imposto lo stato prenotata, non ancora confermata 
            };

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            var appointmentDto = new AppointmentDto
            {
                Id = appointment.Id,
                DateTimeAppointment = appointment.DateTimeAppointment,
                Status = appointment.Status,
                DoctorName = doctorExist.Name + " " + doctorExist.Lastname,     // riusati, niente Include
                PatientName = patientExist.Name + " " + patientExist.Lastname
            };

            return CreatedAtAction(nameof(GetById), new { id = appointmentDto.Id }, appointmentDto);
        }

        // dentro AppointmentsController.cs
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, AppointmentUpdateDto updatedAppointmentDto)
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);

            if (existingAppointment == null)
            {
                return NotFound();
            }

            existingAppointment.DateTimeAppointment = updatedAppointmentDto.DateTimeAppointment;
            existingAppointment.Status = updatedAppointmentDto.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var existingAppointment = await _context.Appointments.FindAsync(id);

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