using ClinicBooking.Api.Models;

namespace ClinicBooking.Api.Dtos
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public DateTime DateTimeAppointment { get; set; }
        public AppointmentStatus Status { get; set; }
        public string DoctorName { get; set; } = string.Empty;   // "appiattito": Doctor.Name + Doctor.Lastname
        public string PatientName { get; set; } = string.Empty;  // stesso principio
    }
}