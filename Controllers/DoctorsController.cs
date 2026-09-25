using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;
using ClinicBooking.Api.Dtos;

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

        // cosa succede nella pratica? 
        // leggi i Doctor dal database come hai sempre fatto, poi per ognuno costruisci un DoctorDto corrispondente e lo metti in una lista nuova
        [HttpGet]
        public async Task<List<DoctorDto>> GetDoctorsAsync()
        {
            var doctors = await _context.Doctors.ToListAsync();   // lista di Doctor (Model) — questa parte non cambia

            var doctorDto = new List<DoctorDto>();   // lista vuota di DoctorDto, da riempire

            foreach (var doctor in doctors)
            {
                doctorDto.Add(new DoctorDto
                {
                    Id = doctor.Id,
                    Name = doctor.Name,
                    Lastname = doctor.Lastname,
                    Specialization = doctor.Specialization,
                    RegistrationDate = doctor.RegistrationDate
                });
                
            }

            return doctorDto;   // restituisci la lista di DTO, non quella di Doctor
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDto>> GetById(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
            {
                return NotFound();
            }

            // a differenza d GetDoctorsAsync, lavoro UN SOLO medico
            var doctorDto = new DoctorDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Lastname = doctor.Lastname,
                Specialization = doctor.Specialization,
                RegistrationDate = doctor.RegistrationDate
            };

            return Ok(doctorDto);

        }

        [HttpPost]
        public async Task<ActionResult<DoctorDto>> AddDoctorAsync(DoctorCreateDto newDoctorDto)
        {
            // 1. Costruisci un Doctor vero a partire dal DTO ricevuto,
            //    aggiungendo i campi che il chiamante NON può decidere
            var doctor = new Doctor
            {
                Name = newDoctorDto.Name,
                Lastname = newDoctorDto.Lastname,
                Specialization = newDoctorDto.Specialization,
                RegistrationDate = DateTime.UtcNow   // deciso dal server, non dal chiamante
            };

            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();   // ora 'doctor' ha anche l'Id, generato dal DB

            // 2. Costruisci il DTO di risposta a partire dal Doctor appena salvato
            var doctorDto = new DoctorDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Lastname = doctor.Lastname,
                Specialization = doctor.Specialization,
                RegistrationDate = doctor.RegistrationDate
            };

            return CreatedAtAction(nameof(GetById), new { id = doctorDto.Id }, doctorDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, DoctorCreateDto updatedDoctorDto)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);

            if (existingDoctor == null)
            {
                return NotFound();
            }

            existingDoctor.Name = updatedDoctorDto.Name;
            existingDoctor.Lastname = updatedDoctorDto.Lastname;
            existingDoctor.Specialization = updatedDoctorDto.Specialization;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);

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