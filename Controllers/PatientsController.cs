using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Api.Data;
using ClinicBooking.Api.Models;
using ClinicBooking.Api.Dtos;

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

        // cosa succede nella pratica? 
        // leggi i Doctor dal database come hai sempre fatto, poi per ognuno costruisci un DoctorDto corrispondente e lo metti in una lista nuova
        [HttpGet]
        public async Task<List<PatientDto>> GetPatientsAsync()
        {
            var patients = await _context.Patients.ToListAsync();   // lista di Doctor (Model) — questa parte non cambia

            var patientDto = new List<PatientDto>();   // lista vuota di DoctorDto, da riempire

            foreach (var patient in patients)
            {
                patientDto.Add(new PatientDto
                {
                    Id = patient.Id,
                    Name = patient.Name,
                    Lastname = patient.Lastname,
                    DateOfBirth = patient.DateOfBirth
                });
            }

            return patientDto;   // restituisci la lista di DTO, non quella di Doctor
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetById(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound();
            }

            // a differenza d GetDoctorsAsync, lavoro UN SOLO medico
            var patientDto = new PatientDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Lastname = patient.Lastname,
                DateOfBirth = patient.DateOfBirth
            };

            return Ok(patientDto);

        }

        [HttpPost]
        public async Task<ActionResult<PatientDto>> AddPatientAsync(PatientCreateDto newPatientDto)
        {
            // 1. Costruisci un Patient vero a partire dal DTO ricevuto,
            //    aggiungendo i campi che il chiamante NON può decidere
            var patient = new Patient
            {
                Name = newPatientDto.Name,
                Lastname = newPatientDto.Lastname,
                DateOfBirth = newPatientDto.DateOfBirth   // quando è nato il paziente  
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();   // ora 'doctor' ha anche l'Id, generato dal DB

            // 2. Costruisci il DTO di risposta a partire dal Doctor appena salvato
            var patientDto = new PatientDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Lastname = patient.Lastname,
                DateOfBirth = patient.DateOfBirth
            };

            return CreatedAtAction(nameof(GetById), new { id = patientDto.Id }, patientDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, PatientCreateDto updatedPatientDto)
        {
            var existingPatient = await _context.Patients.FindAsync(id);

            if (existingPatient == null)
            {
                return NotFound();
            }

            existingPatient.Name = updatedPatientDto.Name;
            existingPatient.Lastname = updatedPatientDto.Lastname;
            existingPatient.DateOfBirth = updatedPatientDto.DateOfBirth;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var existingPatient = await _context.Patients.FindAsync(id);

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