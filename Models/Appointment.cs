namespace ClinicBooking.Api.Models
{
    public enum AppointmentStatus
    {
        booked, // prenotata, non ancora confermata
        confirmed, // confermata dal medico/segreteria
        complete // conclusa
    }

    public class Appointment
    {
        public int Id { get; set; }
        public DateTime DateTimeAppointment { get; set; }
        public AppointmentStatus Status { get; set; }
        public int DoctorId { get; set; } // FK 
        public Doctor? Doctor { get; set; } // navigation property verso Doctor
        // nullable: valorizzata solo se caricata con Include()
        public int PatientId { get; set; } // FK 
        // nullable: valorizzata solo se caricata con Include()
        public Patient? Patient { get; set; } // navigation property verso Patient

    }
}