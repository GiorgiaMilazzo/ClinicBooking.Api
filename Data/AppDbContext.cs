using ClinicBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Api.Data
{
    // costruttore
    // <AppDbContext>: distingue queste opzioni da quelle di eventuali altri DbContext nel progetto
    public class AppDbContext(DbContextOptions<AppDbContext> options ) : DbContext(options)
    {
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; } 
        public DbSet<User> Users { get; set; }
    }
}